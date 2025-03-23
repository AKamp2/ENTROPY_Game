using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System;

public class ZeroGravity : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private CapsuleCollider boundingSphere;
    [SerializeField]
    public Camera cam;
    //Variables for camera turn and UI interaction
    [SerializeField]
    private GameObject characterPivot;
    [SerializeField]
    private UnityEngine.UI.Image crosshair;
    [SerializeField]
    private UnityEngine.UI.Image grabber;
    [SerializeField]
    private Sprite openHand;
    [SerializeField]
    private Sprite closedHand;
    [SerializeField]
    private Sprite crosshairIcon;

    [SerializeField]
    private float sensitivityX = 8.0f;
    [SerializeField]
    private float sensitivityY = 8.0f;

    //rotation variables to track how the camera is rotated
    private float rotationHoriz = 0.0f;
    private float rotationVert = 0.0f;
    private float rotationZ = 0.0f;

    public GameObject respawnLoc;

    /*    // Smooth rotation variables
        private float targetRotationHoriz = 0.0f;
        private float targetRotationVert = 0.0f;
        private float targetRotationZ = 0.0f;

        [SerializeField]
        private float rotationSmoothTime = 0.1f; // Adjust this value to control the smoothness (higher = slower, more floaty)
        [SerializeField]
        private float rollSmoothTime = 0.15f; // Slightly slower smooth time for roll to make it feel floatier

        private float currentVelocityX = 0.0f;
        private float currentVelocityY = 0.0f;
        private float currentVelocityZ = 0.0f;*/

    private bool canMove = true;

    [Header("== Player Movement Settings ==")]
    [SerializeField]
    public float speed = 50.0f;
    [SerializeField]
    private float rollTorque = 250.0f;
    private float currentRollSpeed = 0f;
    [SerializeField]
    private float rollAcceleration = 10f; // How quickly it accelerates to rollTorque
    [SerializeField]
    private float rollFriction = 5f; // How quickly it decelerates when input stops

    [Header("== Grabbing Settings ==")]
    // Grabbing mechanic variables
    private bool isGrabbing = false;
    private Transform potentialGrabbedBar = null; //tracks a potential grabbable bar that the player looks at
    private Transform grabbedBar; //stores the bar the player is currently grabbing
    [SerializeField]
    private LayerMask barLayer; // Set a specific layer containing bars to grab onto
    [SerializeField]
    private LayerMask barrierLayer; //set layer for barriers
    [SerializeField]
    private float grabRange = 3f; // Range within which the player can grab bars
    [SerializeField]
    private float grabPadding = 50f;
    //Propel off bar 
    [SerializeField]
    private float propelThrust = 50000f;
    [SerializeField]
    private float propelOffWallThrust = 50000f;


    [Header("== UI Settings ==")]
    [SerializeField]
    private TextMeshProUGUI grabUIText;
    private bool showTutorialMessages = true;

    //Input Values
    public InputActionReference grab;
    private float thrust1D;
    private float strafe1D;
    private float offWall;
    private bool nearBarrier;

    [SerializeField]
    private DoorManager doorManager;


    // Track if the movement keys were released
    private bool movementKeysReleased;
    private bool spaceKeyReleased;

    //Properties
    //this property allows showTutorialMessages to be assigned outside of the script. Needed for the tutorial mission
    public bool ShowTutorialMessages
    {
        get { return showTutorialMessages; }
        set { showTutorialMessages = value; }
    }

    public bool CanMove
    {
        get { return canMove; }
        set { canMove = value; }
    }

    // getter for isGrabbing
    public bool IsGrabbing => isGrabbing;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        rb.useGravity = false;
        cam = Camera.main;

        //set the crosshair and grabber sprites accordingly;
        crosshair.sprite = crosshairIcon;


        grabber.sprite = null;
        grabber.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);

        doorManager = FindObjectOfType<DoorManager>();
    }

    // Update is called once per frame
    #region Update Method
    void FixedUpdate()
    {
        if (canMove)
        {
            RotateCam();
            HandleRaycast();
            HandleGrabMovement();
            DetectBarrierAndBounce();
            //handle grabber icon logic
            if (isGrabbing && grabbedBar != null)
            {
                //keep grabber locked to grabbed bar
                UpdateGrabberPosition(grabbedBar);
            }
            else
            {
                //update to closest bar in view 
                UpdateClosestBarInView();
            }
        }
    }
    #endregion 

    #region Player Control Methods
    private void RotateCam()
    {
        // Horizontal and vertical rotation
        cam.transform.Rotate(Vector3.up, rotationHoriz * sensitivityX * Time.deltaTime);
        cam.transform.Rotate(Vector3.right, -rotationVert * sensitivityY * Time.deltaTime);

        // Apply roll rotation (Z-axis)
        if (Mathf.Abs(rotationZ) > 0.1f) // Only apply roll if rotationZ input is significant
        {
            // Calculate target roll direction and speed based on input
            float targetRollSpeed = -Mathf.Sign(rotationZ) * rollTorque;

            // Gradually increase currentRollSpeed towards targetRollSpeed
            currentRollSpeed = Mathf.MoveTowards(currentRollSpeed, targetRollSpeed, rollAcceleration * Time.deltaTime);
        }
        else if (Mathf.Abs(currentRollSpeed) > 0.1f) // Apply friction when no input
        {
            // Gradually decrease currentRollSpeed towards zero
            currentRollSpeed = Mathf.MoveTowards(currentRollSpeed, 0f, rollFriction * Time.deltaTime);
        }

        // Apply the roll rotation to the camera
        cam.transform.Rotate(Vector3.forward, currentRollSpeed * Time.deltaTime);
    }

    private void PropelOffWall()
    {
        // Check space button is currently being pressed 
        bool isPushing = Mathf.Abs(offWall) > 0.1f;

        if(spaceKeyReleased && isPushing)
        {
            Vector3 propelDirection = Vector3.zero;

            propelDirection += -cam.transform.forward * offWall * propelOffWallThrust;

            rb.AddForce(propelDirection * Time.deltaTime, ForceMode.VelocityChange);
            // Set the flag to false since keys are now pressed
            spaceKeyReleased = false;
        }
        //update the flag if the space key is not being pressed
        else if(!isPushing)
        {

            spaceKeyReleased = true;
        }
    }

    private void DetectBarrierAndBounce()
    {
        float detectionRadius = boundingSphere.radius + 0.3f; // Slightly larger for early detection
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius, barrierLayer);

        if (hitColliders.Length > 0)
        {
            Vector3 totalBounceDirection = Vector3.zero;
            Vector3 strongestBounce = Vector3.zero;
            float originalSpeed = rb.velocity.magnitude; // Store initial velocity magnitude

            foreach (Collider barrier in hitColliders)
            {
                Vector3 closestPoint = barrier.ClosestPoint(transform.position);
                Vector3 wallNormal = (transform.position - closestPoint).normalized;
                Vector3 reflectDirection = Vector3.Reflect(rb.velocity.normalized, wallNormal);

                totalBounceDirection += reflectDirection;

                //track the first/strongest reflection in case of near-zero average
                if (strongestBounce == Vector3.zero)
                {
                    strongestBounce = reflectDirection;
                }
            }

            totalBounceDirection.Normalize(); // Get an averaged bounce direction

            //if the total bounce results near zero use strongest bounce
            if (totalBounceDirection.magnitude < 0.1f)
            {
                totalBounceDirection = strongestBounce;
            }

            // Maintain the original speed but reduce slightly to prevent infinite bouncing energy gain
            float bounceSpeed = originalSpeed * 0.7f; // 70% of initial speed to prevent gaining energy

            // Apply new velocity
            rb.velocity = totalBounceDirection * bounceSpeed;
            //rb.angularVelocity = Vector3.zero;

            Debug.Log($"Bounce Direction: {totalBounceDirection}, Speed After Bounce: {rb.velocity.magnitude}");
        }
    }

    /// <summary>
    /// Simple method that only allows player to propel off a bar if they are currently grabbing it
    /// </summary>
    /// <param name="horizontalAxisPos"></param>
    /// <param name="verticalAxisPos"></param>
    private void HandleGrabMovement()
    {
        //Propel off bar logic
        if (isGrabbing)
        {
            currentRollSpeed = 0.0f;
            PropelOffBar();
        }
    }

    private void HandleDoorInteraction(Transform button)
    {
        //store the gameobject of the detected item and store it
        GameObject door = button.parent.gameObject;
        //set the selected door in the door manager as this door
        doorManager.CurrentSelectedDoor = door;
        //show the door UI
        doorManager.DoorUI.SetActive(true);
    }

    private void UpdateClosestBarInView()
    {
        //check for all nearby bars to the player
        Collider[] nearbyBars = Physics.OverlapSphere(transform.position, grabRange, barLayer);
        //initialize a transform for the closest bar and distance to that bar
        Transform closestBar = null;
        float closestDistance = Mathf.Infinity;

        //check through each bar in our array
        foreach (Collider bar in nearbyBars)
        {
            //set specifications for the viewport
            Vector3 viewportPoint = cam.WorldToViewportPoint(bar.transform.position);

            //check if the bar is in the viewport and in front of the player
            if (viewportPoint.z > 0 && viewportPoint.x >= 0 && viewportPoint.x <= 1 && viewportPoint.y >= 0 && viewportPoint.y <= 1)
            {
                float distanceToBar = Vector3.Distance(transform.position, bar.transform.position);
                if (distanceToBar < closestDistance)
                {
                    closestDistance = distanceToBar;
                    closestBar = bar.transform;
                }
            }
        }

        if (closestBar != null)
        {
            // Update the grabber if a new bar is detected
            if (potentialGrabbedBar != closestBar)
            {
                potentialGrabbedBar = closestBar;
                UpdateGrabberPosition(potentialGrabbedBar);
            }
        }
        else
        {
            // Hide grabber if no bar is in range
            HideGrabber();
        }
    }

    // this method will update the grabber icon's position based on the nearest grabbable object
    private void UpdateGrabberPosition(Transform bar)
    {
        //check if their is a bar in the viewport
        if (bar != null)
        {
            //set it as a screen point
            Vector3 screenPoint = cam.WorldToScreenPoint(bar.position);

            // Update grabber position
            grabber.rectTransform.position = screenPoint;

            // Set hand icon to open by default
            if (!isGrabbing)
            {
                grabber.sprite = openHand;
                grabber.color = Color.white;
            }
            else if (isGrabbing)
            {
                grabber.sprite = closedHand;
                grabber.color = Color.white;
            }
        }
        //if there is no bar
        else
        {
            //remove the grabber
            HideGrabber();
        }
    }

    // this method removes the grabber sprite from the screen. making sure there are no floating grabbers in the ui
    public void HideGrabber()
    {
        grabber.sprite = null;
        grabber.color = new Color(0, 0, 0, 0); //transparent
    }

    public void GrabBar()
    {
            isGrabbing = true;
            grabbedBar = potentialGrabbedBar;

            //lock grabbed bar and change icon
            UpdateGrabberPosition(grabbedBar);
            grabber.sprite = closedHand;

            //set the velocities to zero so that the player stops when they grab the bar
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
    }

    // Release the bar and enable movement again
    private void ReleaseBar()
    {
            isGrabbing = false;
            grabbedBar = null;
            Debug.Log("Released the handle");

            //resume dynamic bar detection
            UpdateClosestBarInView();
    }

    private void ResetUI()
    {
        grabUIText.text = null;
        grabber.sprite = null;
        grabber.color = new Color(0, 0, 0, 0);
        /*doorManager.DoorUI.SetActive(false);*/
    }

    //Player uses WASD to propel themselves faster, only while currently grabbing a bar
    private void PropelOffBar()
    {
        //if the player is grabbing and no movement buttons are currently being pressed
        if (isGrabbing)
        {
            // Check if no movement buttons are currently being pressed
            bool isThrusting = Mathf.Abs(thrust1D) > 0.1f;
            bool isStrafing = Mathf.Abs(strafe1D) > 0.1f;

            if (movementKeysReleased && (isThrusting || isStrafing))
            {
                //initialize a vector 3 for the propel direction
                Vector3 propelDirection = Vector3.zero;

                //if W or S are pressed
                if (isThrusting)
                {
                    //release the bar and calculate the vector to propel based on the forward look
                    ReleaseBar();
                    propelDirection += cam.transform.forward * thrust1D * propelThrust;
                    Debug.Log("Propelled forward or back");
                }
                //if A or D are pressed
                else if (isStrafing)
                {
                    //release the bar and calculate the vector to propel based on the right look
                    ReleaseBar();
                    propelDirection += cam.transform.right * strafe1D * propelThrust;
                    Debug.Log("Propelled right or left");
                }
                //add the propel force to the rigid body
                rb.AddForce(propelDirection * Time.deltaTime, ForceMode.VelocityChange);
                // Set the flag to false since keys are now pressed
                movementKeysReleased = false;
            }
            // Update the flag if no movement keys are pressed
            else if (!isThrusting && !isStrafing)
            {
                movementKeysReleased = true;
            }
        }
    }

    private void HandleRaycast()
    {
        if (isGrabbing)
        {
            //skip raycast if already holding a bar
            return;
        }

        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * grabRange, Color.red, 0.1f); // Debug visualization

        if (Physics.Raycast(ray, out hit, grabRange, barLayer | barrierLayer))
        {
            Debug.Log("Hit: " + hit.transform.name + " | Tag: " + hit.transform.tag); // Debugging

            //use the helper methods to manage the player ui
            RayCastHandleGrab(hit);

            RayCastHandleBounce(hit);

            RayCastHandleDoorButton(hit);
        }
        else
        {
            ResetUI();
            potentialGrabbedBar = null;
        }
    }

    //helper methods for raycast handling
    public void RayCastHandleGrab(RaycastHit hit)
    {
        if (hit.transform.CompareTag("Grabbable"))
        {
            potentialGrabbedBar = hit.transform;
            UpdateGrabberPosition(potentialGrabbedBar);
        }
    }

    public void RayCastHandleBounce(RaycastHit hit)
    {
        if (hit.transform.CompareTag("Barrier"))
        {
            Debug.Log("Barrier detected: " + hit.transform.name);
            grabUIText.text = "'SPACEBAR'";
            //if looking at the wall, press space to push off of
            if (offWall > 0.1f)
            {
                PropelOffWall();
                Debug.Log("Propeled off wall");
            }
        }
    }

    public void RayCastHandleDoorButton(RaycastHit hit)
    {
        //need this to send to UI manager
        if (hit.transform.CompareTag("DoorButton"))
        {
            //show door UI
            HandleDoorInteraction(hit.transform);
        }
    }

    #endregion

    void OnDrawGizmos()
    {
        // Visualize the crosshair padding as a box in front of the camera
        if (cam != null)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, crosshair.rectTransform.position);

            // Define padded bounds
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            Vector2 paddedMin = new Vector2(screenPoint.x - grabPadding, screenPoint.y - grabPadding);
            Vector2 paddedMax = new Vector2(screenPoint.x + grabPadding, screenPoint.y + grabPadding);

            // Draw a box at the grab range with padding
            Gizmos.color = Color.green;
            for (float x = paddedMin.x; x <= paddedMax.x; x += grabPadding / 2)
            {
                for (float y = paddedMin.y; y <= paddedMax.y; y += grabPadding / 2)
                {
                    Ray ray = cam.ScreenPointToRay(new Vector3(x, y, 0));
                    Gizmos.DrawRay(ray.origin, ray.direction * grabRange);
                }
            }
        }
    }

    #region Input Methods
    //when we press the buttons on the keyboard or controller these methods pass the buttons through to read the values
    //MUST MANUALLY SET THE CONNECTIONS IN THE EVENTS PANEL ONCE ADDED A PLAYER INPUT COMPONENT
    public void OnMouseX(InputAction.CallbackContext context)
    {
        rotationHoriz = context.ReadValue<float>();
    }

    public void OnMouseY(InputAction.CallbackContext context)
    {
        rotationVert = context.ReadValue<float>();
    }

    public void OnThrust(InputAction.CallbackContext context)
    {
        /*once the button, Keyboard or Controller, that is passed through the
         Player Input event to this value of thrust1D*/
        thrust1D = context.ReadValue<float>();
    }

    public void OnStrafe(InputAction.CallbackContext context)
    {
        strafe1D = context.ReadValue<float>();
    }
    public void OffWall(InputAction.CallbackContext context)
    {
        offWall = context.ReadValue<float>();
    }
    public void OnRoll(InputAction.CallbackContext context)
    {
        rotationZ = context.ReadValue<float>();
    }
    public void OnGrab(InputAction.CallbackContext context)
    {
        if (context.performed && potentialGrabbedBar != null)
        {
            GrabBar();
        }
        else if (context.canceled)
        {
            ReleaseBar();
        }
    }
    #endregion
}
