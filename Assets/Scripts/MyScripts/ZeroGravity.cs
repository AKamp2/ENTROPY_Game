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

    private void PropelOffWall(Vector3 wallNormal)
    {
        Vector3 propelDirection = Vector3.zero;

        propelDirection += -cam.transform.forward * offWall * propelOffWallThrust;

        rb.AddForce(propelDirection * Time.deltaTime, ForceMode.VelocityChange);
        Debug.Log("Propelled away from wall");
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

    // Try to grab a bar by raycasting
    private void TryGrabBar(Transform bar)
    {
        grabbedBar = bar;
        isGrabbing = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        grabber.sprite = closedHand;
    }

    // This method will update the crosshair's position based on the nearest grabbable object
    private void UpdateGrabberPosition(Transform rayPt)
    {
        //create a screenpoint based on the raycast hit
        Vector3 screenPoint = cam.WorldToScreenPoint(rayPt.position);
        grabber.rectTransform.position = screenPoint;

        //handle UI hand updating
        if (!isGrabbing)
        {
            grabber.sprite = openHand;
            grabber.color = Color.white;
            grabUIText.text = "press and hold 'Right Mouse Button'";
        }
        if (isGrabbing)
        {
            grabber.sprite = closedHand;
            grabber.color = Color.white;
            grabUIText.text = "WASD";
        }
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

    // Release the bar and enable movement again
    private void ReleaseBar()
    {
        isGrabbing = false;
        grabbedBar = null;
        Debug.Log("Released the handle");

    }

    private void HandleRaycast()
    {
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * grabRange, Color.red, 0.1f); // Debug visualization

        if (Physics.Raycast(ray, out hit, grabRange, barLayer | barrierLayer))
        {
            Debug.Log("Hit: " + hit.transform.name + " | Tag: " + hit.transform.tag); // Debugging

            if (hit.transform.CompareTag("Grabbable"))
            {
                //update the ui hand to be on the bar player is looking at and in range of
                UpdateGrabberPosition(hit.transform);
                //store the potential bar for grabbing
                potentialGrabbedBar = hit.transform;
            }
            if (hit.transform.CompareTag("Barrier"))
            {
                Debug.Log("Barrier detected: " + hit.transform.name);
                grabUIText.text = "'SPACEBAR'";
                //if looking at the wall, press space to push off of
                if (offWall > 0.1f)
                {
                    PropelOffWall(hit.normal);
                    Debug.Log("Propeled off wall");
                }
            }
            //need this to send to UI manager
            if (hit.transform.CompareTag("DoorButton"))
            {
                //show door UI
                HandleDoorInteraction(hit.transform);
            }
        }
        else
        {
            ResetUI();
            potentialGrabbedBar = null;
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
            TryGrabBar(potentialGrabbedBar);
        }
        else if (context.canceled)
        {
            ReleaseBar();
        }
    }
    #endregion
}
