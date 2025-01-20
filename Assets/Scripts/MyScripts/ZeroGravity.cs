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
    private Transform grabbedBar;
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
    private float upDown1D;
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
    void FixedUpdate()
    {
        if (canMove)
        {
            RotateCam();
            LookingAtButton();
            HandleGrabMovement();
            PropelOffWall();
            UpdateGrabberPosition();
            UIText();
        }
    }

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
        // Adjust the forward thrust based on your requirements
        Vector3 propelDirection = cam.transform.forward * propelOffWallThrust;

        if (nearBarrier && upDown1D > 0.1f)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, crosshair.rectTransform.position);

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            Vector2 paddedMin = new Vector2(screenPoint.x - grabPadding, screenPoint.y - grabPadding);
            Vector2 paddedMax = new Vector2(screenPoint.x + grabPadding, screenPoint.y + grabPadding);

            bool barrierHit = false; // Track if a barrier was hit

            for (float x = paddedMin.x; x < paddedMax.x; x += grabPadding / 2)
            {
                for (float y = paddedMin.y; y < paddedMax.y; y += grabPadding / 2)
                {
                    Ray ray = cam.ScreenPointToRay(new Vector3(x, y, 0));
                    RaycastHit hit;

                    // Check if the raycast hits a barrier
                    if (Physics.Raycast(ray, out hit, boundingSphere.radius, barrierLayer) && hit.transform.CompareTag("Barrier"))
                    {
                        barrierHit = true; // Mark that we hit a barrier
                        break;
                    }
                }

                if (barrierHit)
                    break;
            }

            if (barrierHit)
            {
                // Propelling away from the wall
                rb.AddForce(-propelDirection * Time.deltaTime, ForceMode.VelocityChange);
                Debug.Log("Propelling away from Barrier");
            }
            else
            {
                // Regular forward propulsion
                rb.AddForce(propelDirection * Time.deltaTime, ForceMode.VelocityChange);
                Debug.Log("Propelling forward, no barrier detected");
            }
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Barrier") && collision.gameObject.CompareTag("Barrier"))
        {
            nearBarrier = true;
            grabUIText.text = "'SPACEBAR'";
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Barrier") && collision.gameObject.CompareTag("Barrier"))
        {
            nearBarrier = false;
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

    /// <summary>
    /// Creates a raycast to find if the player is within range of the bar in front of them
    /// </summary>
    /// <returns></returns>
    private bool IsInRangeofBar()
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, crosshair.rectTransform.position);
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        Vector2 paddedMin = new Vector2(screenPoint.x - grabPadding, screenPoint.y - grabPadding);
        Vector2 paddedMax = new Vector2(screenPoint.x + grabPadding, screenPoint.y + grabPadding);

        for (float x = paddedMin.x; x < paddedMax.x; x += grabPadding / 2)
        {
            for (float y = paddedMin.y; y < paddedMax.y; y += grabPadding / 2)
            {
                Ray ray = cam.ScreenPointToRay(new Vector3(x, y, 0));
                if (Physics.Raycast(ray, grabRange, barLayer))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void LookingAtButton()
    {

        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.TransformDirection(Vector3.forward), out hit, 3.0f))
        {
            if (hit.transform.gameObject.tag == "DoorButton" && doorManager.CurrentSelectedDoor != hit.transform.parent.gameObject)
            {
               
                GameObject g = hit.transform.parent.gameObject;
                DoorScript ds = g.GetComponent<DoorScript>();

                Debug.Log(ds);

                if (ds.DoorState != DoorScript.States.Locked && ds.DoorState != DoorScript.States.Broken)
                {
                    doorManager.CurrentSelectedDoor = g;
                    doorManager.DoorUI.SetActive(true);

                }
                else
                {
                    doorManager.CurrentSelectedDoor = null;
                }

                

            }
            else if (hit.transform.gameObject.tag != "DoorButton" && doorManager.CurrentSelectedDoor != null)
            {
                doorManager.DoorUI.SetActive(false);
                doorManager.CurrentSelectedDoor = null;
            }
          
        }
        else
        {
            doorManager.DoorUI.SetActive(false);
            doorManager.CurrentSelectedDoor = null;
        }




    }

    // Try to grab a bar by raycasting
    private void TryGrabBar()
    {

        // Get the position of the crosshair in screen space
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, crosshair.rectTransform.position);

        // Apply padding to the screen point
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        Vector2 paddedMin = new Vector2(screenPoint.x - grabPadding, screenPoint.y - grabPadding);
        Vector2 paddedMax = new Vector2(screenPoint.x + grabPadding, screenPoint.y + grabPadding);

        for (float x = paddedMin.x; x < paddedMax.x; x += grabPadding / 2)
        {
            for (float y = paddedMin.y; y < paddedMax.y; y += grabPadding / 2)
            {
                Ray ray = cam.ScreenPointToRay(new Vector3(x, y, 0));
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, grabRange, barLayer))
                {
                    Debug.Log("Raycast hit: " + hit.transform.name); // Debug the hit

                    // Check if the object has the "Grabbable" tag
                    if (hit.transform.CompareTag("Grabbable"))
                    {
                        grabbedBar = hit.transform;
                        isGrabbing = true;

                        // Stop movement by reducing velocity and angular velocity
                        rb.velocity = rb.velocity * 0f;  // Reduce velocity to 10% of its current value
                        rb.angularVelocity = Vector3.zero;  // Stop rotation completely

                        Debug.Log("Grabbing the handle: " + grabbedBar.name);
                        return;
                    }
                }
            }
        }
        Debug.Log("Raycast did not hit anything");
    }

    // This method will update the crosshair's position based on the nearest grabbable object
    private void UpdateGrabberPosition()
    {
        Transform closestObject = GetClosestGrabbableObject();

        if (closestObject != null && !isGrabbing)
        {
            // Convert the closest object's world position to screen position
            Vector3 screenPoint = cam.WorldToScreenPoint(closestObject.transform.position);

            // Update the grabber's position to match the object's screen position
            grabber.rectTransform.position = screenPoint;
            //set the hand icon on the bar
            grabber.sprite = openHand;
            grabber.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }
        else if(isGrabbing)
        {
            // Convert the closest object's world position to screen position
            Vector3 screenPoint = cam.WorldToScreenPoint(closestObject.transform.position);

            // Update the grabber's position to match the object's screen position
            grabber.rectTransform.position = screenPoint;
            //set the closed hand icon on the bar
            grabber.sprite = closedHand;
            grabber.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }
        else 
        {
            // If no grabbable object is within range, reset grabber to center of the screen
           grabber.rectTransform.position = new Vector2(Screen.width / 2, Screen.height / 2);
            //set the grabber to null
            grabber.sprite = null;
            grabber.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        }
    }

    // Method to find the closest grabbable object within grab range
    private Transform GetClosestGrabbableObject()
    {
        RaycastHit[] hits = Physics.SphereCastAll(cam.transform.position, grabRange, cam.transform.forward, grabRange, barLayer);
        Transform closestObject = null;
        float closestDistance = Mathf.Infinity;

        if (IsInRangeofBar())
        {
            foreach (RaycastHit hit in hits)
            {
                if (hit.transform.CompareTag("Grabbable"))
                {
                    float distance = Vector3.Distance(cam.transform.position, hit.transform.position);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestObject = hit.transform;
                    }
                }
            }
        }

        return closestObject;
    }


    private void UIText()
    {
        if (showTutorialMessages)
        {
            if (isGrabbing)
            {
                grabUIText.text = "'WASD'";
            }
            else if (IsInRangeofBar() && !isGrabbing)
            {
                grabUIText.text = "press and hold 'Right Mouse Button'";
            }
            else if (!isGrabbing && !IsInRangeofBar() && !nearBarrier)
            {
                grabUIText.text = null;
            }
        }
        else if (!showTutorialMessages)
        {
            return;
        }
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
    public void OnUpDown(InputAction.CallbackContext context)
    {
        upDown1D = context.ReadValue<float>();
    }
    public void OnRoll(InputAction.CallbackContext context)
    {
        rotationZ = context.ReadValue<float>();
    }
    public void OnGrab(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            TryGrabBar();
        }
        else if (context.canceled)
        {
            ReleaseBar();
        }
    }
    #endregion
}
