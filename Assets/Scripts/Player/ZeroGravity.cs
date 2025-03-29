using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System;

public class ZeroGravity : MonoBehaviour
{
    [Header("== Player Elements ==")]
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private CapsuleCollider boundingSphere;
    [SerializeField]
    public Camera cam;


    //look sensitivity
    [SerializeField]
    private float sensitivityX = 8.0f;
    [SerializeField]
    private float sensitivityY = 8.0f;

    //rotation variables to track how the camera is rotated
    private float rotationHoriz = 0.0f;
    private float rotationVert = 0.0f;
    private float rotationZ = 0.0f;

    //respawn reference
    public GameObject respawnLoc;

    //player health trackers
    [SerializeField]
    private int playerHealth = 4;
    [SerializeField]
    private int maxHealth = 4;
    private bool isDead = false;

    //win tracker
    private bool didWin = false;
    [SerializeField]
    private Collider end;

    //handle taking damage
    private bool justHit = false;
    private bool prevJustHit = false;
    private float justHitCoolDown = .6f;
    private float justHitTimeStamp = 0f;

    //health indicator cooldown
    private bool hurt = false;
    private bool prevHurt = false;
    private float hurtCoolDown = 3.5f;
    private float highDangerCoolDown = 5.0f;
    private float hurtTimeStamp = 0f;

    [Header("== UI Canvas ==")]
    //canvas elements
    [SerializeField]
    private GameObject characterPivot;
    [SerializeField]
    private UnityEngine.UI.Image crosshair;
    [SerializeField]
    private UnityEngine.UI.Image grabber;
    [SerializeField]
    private UnityEngine.UI.Image inputIndicator;
    [SerializeField]
    private UnityEngine.UI.Image healthIndicator;

    //sprite assets
    //grabber
    [SerializeField]
    private Sprite openHand;
    [SerializeField]
    private Sprite closedHand;
    [SerializeField]
    private Sprite crosshairIcon;

    //input indicators
    [SerializeField]
    private Sprite wasdIndicator;
    [SerializeField]
    private Sprite spaceIndicator;
    [SerializeField]
    private Sprite rightClickIndicator;
    [SerializeField]
    private Sprite leftClickIndicator;
    [SerializeField]
    private Sprite keyFIndicator;

    //health indicators
    [SerializeField]
    private Sprite dangerIndicator;
    [SerializeField]
    private Sprite highDangerIndicator; 

    //used for freezing the camera movement while completing the puzzle.
    private bool canMove = true;

    //Fields for the tutorial
    [SerializeField]
    private bool tutorialMode = false;
    private bool canGrab = false;
    private bool canPropel = false;
    private bool canPushOff = false;
    private bool canRoll = false;
    private bool hasPropelled = false;


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
    private LayerMask doorLayer;
    [SerializeField]
    private float grabRange = 3f; // Range within which the player can grab bars
    [SerializeField]
    private float grabPadding = 50f;
    //Propel off bar 
    [SerializeField]
    private float propelThrust = 50000f;

    [Header("== Push Off Wall Settings ==")]
    [SerializeField]
    private float propelOffWallThrust = 50000f;
    private Transform potentialWall = null;


    [SerializeField]
    private float pushSpeed = 1.5f;
    [SerializeField]
    private float dangerSpeed = 10f;
    [SerializeField]
    private float mediumSpeed = 5f;
    [SerializeField]
    private float minimumSpeed = 1f;


    [Header("== UI Settings ==")]
    [SerializeField]
    private TextMeshProUGUI grabUIText;
    private bool showTutorialMessages = true;

    //Input Values
    [SerializeField]
    private InputActionReference grab;
    [SerializeField]
    private InputActionReference pushOffWall;
    private float thrust1D;
    private float strafe1D;


    [Header("== World Element Managers ==")]
    [SerializeField]
    private DoorManager doorManager;
    [SerializeField]
    private TutorialManager tutorialManager;

    // Track if the movement keys were released
    private bool movementKeysReleased;

    //Properties
    //this property allows showTutorialMessages to be assigned outside of the script. Needed for the tutorial mission
    public bool ShowTutorialMessages
    {
        get { return showTutorialMessages; }
        set { showTutorialMessages = value; }
    }

    public bool TutorialMode
    {
        get { return tutorialMode; }
        set { tutorialMode = value; }
    }

    public bool CanMove
    {
        get { return canMove; }
        set { canMove = value; }
    }

    public bool CanGrab
    {
        get { return canGrab; }
        set { canGrab = value; }
    }

    public bool CanPropel
    {
        get { return canPropel; }
        set { canPropel = value; }
    }

    public bool CanPushOff
    {
        get { return canPushOff; }
        set { canPushOff = value; }
    }

    public bool CanRoll
    {
        get { return canRoll; }
        set { canRoll = value; }
    }

    public bool HasPropelled
    {
        get { return hasPropelled;  }
        set { hasPropelled = value;  }
    }

    public bool IsDead
    {
        get { return isDead; }
    }

    // getter for isGrabbing
    public bool IsGrabbing => isGrabbing;

    // Start is called before the first frame update
    void Start()
    {
        //initial player booleans set
        canGrab = true;
        canPushOff = true;
        canPropel = true;
        canRoll = true;
        isDead = false;

        justHit = false;
        prevJustHit = false;
        hurt = false;
        prevHurt = false;


        //let the mouse move
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        rb.useGravity = false;
        cam = Camera.main;

        //set the crosshair and grabber sprites accordingly;
        crosshair.sprite = crosshairIcon;

        //erase the grabber
        grabber.sprite = null;
        grabber.color = new Color(0, 0, 0, 0); //transparent
        //erase the input indicator
        inputIndicator.sprite = null;
        inputIndicator.color = new Color(0, 0, 0, 0); // transparent
        //hide the health indicator
        healthIndicator.sprite = null;
        healthIndicator.color = new Color(0, 0, 0, 0); //transparent

        doorManager = FindObjectOfType<DoorManager>();
        tutorialManager = FindObjectOfType<TutorialManager>();

        //set the player health
        playerHealth = maxHealth;
    }

    // Update is called once per frame
    #region Update Method
    void FixedUpdate()
    {
        if (canMove)
        {
            if (tutorialMode)
            {
                
                RotateCam();
                if (canGrab)
                {
                    HandleGrabMovement();
                }
                HandleRaycast();
                //handle grabber icon logic
                if (isGrabbing && grabbedBar != null && canGrab)
                {
                    //keep grabber locked to grabbed bar
                    UpdateGrabberPosition(grabbedBar);
                    //grabUIText.text = "'W A S D'";
                    //set the sprite for input indicator to the wasd indicator
                    if (canPropel)
                    {
                        inputIndicator.sprite = wasdIndicator;
                        inputIndicator.color = new Color(256, 256, 256, 0.5f);
                    }
                    
                }
                else
                {
                    //update to closest bar in view 
                    UpdateClosestBarInView();
                }
            }
            else
            {
                //Debug.Log("Tutorial Mode off");
                
                RotateCam();
                HandleGrabMovement();
                HandleRaycast();
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
            DetectBarrierAndBounce();
            DetectClosingDoorTakeDamageAndBounce();
            //track the player health and update the ui based on what health the player is on
            HandleHealthUI();

            //manage the cooldowns
            HurtCoolDown();
            JustHitCoolDown();
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
        if (canRoll)
        {
            if (Mathf.Abs(rotationZ) > 0.1f) //only apply roll if rotationZ input is significant
            {
                //calculate target roll direction and speed based on input
                float targetRollSpeed = -Mathf.Sign(rotationZ) * rollTorque;

                //gradually increase currentRollSpeed towards targetRollSpeed
                currentRollSpeed = Mathf.MoveTowards(currentRollSpeed, targetRollSpeed, rollAcceleration * Time.deltaTime);
            }
            else if (Mathf.Abs(currentRollSpeed) > 0.1f) // Apply friction when no input
            {
                //gradually decrease currentRollSpeed towards zero
                currentRollSpeed = Mathf.MoveTowards(currentRollSpeed, 0f, rollFriction * Time.deltaTime);
            }

            //apply the roll rotation to the camera
            cam.transform.Rotate(Vector3.forward, currentRollSpeed * Time.deltaTime);
        }
    }

    private void PropelOffWall()
    {
        if(rb.velocity.magnitude <= pushSpeed && canPushOff)
        {
            //zero the initial velocities ensuring a direct push back
            //rb.velocity = Vector3.zero;
            //rb.angularVelocity = Vector3.zero;
            //create a vector for the new velocity
            Vector3 propelDirection = Vector3.zero;
            propelDirection -= cam.transform.forward * propelOffWallThrust;

            rb.AddForce(propelDirection * Time.deltaTime, ForceMode.VelocityChange);
        }
    }

    private void DetectBarrierAndBounce()
    {
        float detectionRadius = boundingSphere.radius + 0.3f; // Slightly larger for early detection
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius, barrierLayer);

        //Debug.Log(hitColliders.Length);

        if (hitColliders.Length == 0)
        {
            return; //no collision, no bounce 
        }

        Vector3 avgBounceDirection = Vector3.zero;
        int bounceCount = 0;
        float ogSpeed = rb.velocity.magnitude; //store initial velocity magnitude

        //Debug.Log(ogSpeed);

        foreach (Collider barrier in hitColliders)
        {
            Vector3 closestPoint = barrier.ClosestPoint(transform.position);
            Vector3 wallNormal = (transform.position - closestPoint).normalized;
            Vector3 reflectDirection = Vector3.Reflect(rb.velocity.normalized, wallNormal);

            //pudh the player away slighly so they won't be stuck colliding with the wall.
            Vector3 pushAway = wallNormal * 0.05f; // small offset to prevent overlap
            transform.position += pushAway;

            //get bounce directions
            avgBounceDirection += reflectDirection;
            bounceCount++;
            //early exit if multiple bounces aren't needed
            if (bounceCount > 1)
            {
                break;
            }
        }

        if (bounceCount > 0)
        {
            avgBounceDirection.Normalize(); // average direction
            float bounceSpeed = ogSpeed * .75f; // keep 75% of initial speed so it doesn't gain 

            Debug.Log("BoounceSpeed: " + bounceSpeed);
            //verify the bounce is faster than minimum speed
            if (bounceSpeed <= minimumSpeed)
            {
                //reset the bounce speed to be the minimum, ensuring constant bounces
                bounceSpeed = minimumSpeed;
            }

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.velocity = avgBounceDirection * bounceSpeed;
        }

        //check if the bounce is a hard bounce and we haven't been previously hit in the last 1.5 seconds
        if (ogSpeed >= dangerSpeed && !justHit && !isDead)
        {
            //decrease the player's health by 1
            DecreaseHealth(3);
            prevJustHit = justHit;
            justHit = true;
            prevHurt = hurt;
            hurt = true;
        }
        else if (ogSpeed >= mediumSpeed && !justHit && !isDead)
        {
            //decrease the player's health by 1
            DecreaseHealth(1);
            prevJustHit = justHit;
            justHit = true;
            prevHurt = hurt;
            hurt = true;
        }
    }

    private void DetectClosingDoorTakeDamageAndBounce()
    {
        float detectionRadius = boundingSphere.radius + 0.3f; // Slightly larger for early detection
        Collider[] hitDoors = Physics.OverlapSphere(transform.position, detectionRadius, doorLayer);

        if (hitDoors.Length == 0)
        {
            return; //no collision, no bounce 
        }

        Vector3 avgBounceDirection = Vector3.zero;
        int bounceCount = 0;
        float ogSpeed = rb.velocity.magnitude; //store initial velocity magnitude

        foreach (Collider door in hitDoors)
        {
            //get the doorscript of the parent object of the door collider
            DoorScript doorScript = door.GetComponentInParent<DoorScript>();


            if (doorScript != null && doorScript.IsClosing)
            {
                //check that the state is a broken door
                if (doorScript.DoorState == DoorScript.States.Broken)
                {
                    bounceCount++;

                    //calculate bounce direction away from the door
                    Vector3 bounceDirection = (transform.position - door.bounds.center).normalized;

                    avgBounceDirection += bounceDirection;

                    //early exit if multiple bounces aren't needed
                    if (bounceCount >= 1)
                    {
                        break;
                    }
                }
            }
        }
        //if we have a bounce to calculate, and we haven't been hit
        if (bounceCount > 0)
        {
            //set velocity to zero
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            //only normalize if avgBounceDirection is valid
            if (avgBounceDirection != Vector3.zero)
            {
                avgBounceDirection.Normalize();
            }

            //Debug.Log("Avg Bounce Direction: " + avgBounceDirection);

            float bounceSpeed = ogSpeed * .3f; // keep 30% of initial speed so it doesn't gain 

            //calculate the direction of the bounce
            Vector3 propelDirection = avgBounceDirection * ogSpeed * (propelThrust * .50f) * Time.deltaTime;
            //Debug.Log("propel direction: " + propelDirection);
            rb.AddForce(propelDirection, ForceMode.VelocityChange);

            if (!isDead && !justHit)
            {
                //decrease the player health after they have collided with the closing door
                DecreaseHealth(4);

                //set just hit to true, commencing cooldown
                prevJustHit = justHit;
                justHit = true;
                prevHurt = hurt;
                hurt = true;
            }
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
            //update the grabber if a new bar is detected
            if (potentialGrabbedBar != closestBar)
            {
                potentialGrabbedBar = closestBar;
                UpdateGrabberPosition(potentialGrabbedBar);
                //if in tutorial mode
                if (tutorialMode && canGrab)
                {
                    //grabUIText.text = "press and hold 'RIGHT MOUSE BUTTON'";
                    //set the sprite for the right click
                    inputIndicator.sprite = rightClickIndicator;
                    inputIndicator.color = new Color(256, 256, 256, 0.5f);
                }
            }
        }
        else
        {
            //hide grabber if no bar is in range
            HideGrabber();
        }
    }

    // this method will update the grabber icon's position based on the nearest grabbable object
    private void UpdateGrabberPosition(Transform bar)
    {
        if (canGrab)
        {
            //check if their is a bar in the viewport
            if (bar != null)
            {
                //set the position of the bar as a screen point
                Vector3 screenPoint = cam.WorldToScreenPoint(bar.position);

                if (screenPoint.z > 0)
                {
                    //update grabber position
                    grabber.rectTransform.position = screenPoint;

                    //set hand icon open is not grabbing
                    if (!isGrabbing)
                    {
                        grabber.sprite = openHand;
                        grabber.color = Color.white;
                    }
                    //set closed hand icon if grabbing
                    else if (isGrabbing)
                    {
                        grabber.sprite = closedHand;
                        grabber.color = Color.white;
                    }
                }
                else
                {
                    //hide the grabber when the bar is behind the camera
                    HideGrabber();
                }

            }
            //if there is no bar
            else
            {
                //remove the grabber
                HideGrabber();
            }
        }
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

        //lock grabbed bar and change icon
        grabber.sprite = openHand;
        inputIndicator.sprite = null;
        inputIndicator.color = new Color(0, 0, 0, 0);

        //resume dynamic bar detection
        UpdateClosestBarInView();
    }

    private void ResetUI()
    {
        grabUIText.text = null;
        //erase the grabber
        grabber.sprite = null;
        grabber.color = new Color(0, 0, 0, 0);
        //erase the input indicator
        inputIndicator.sprite = null;
        inputIndicator.color = new Color(0, 0, 0, 0);
        /*doorManager.DoorUI.SetActive(false);*/
    }

    //Health Methods
    //controls the UI for the Player Health
    private void HandleHealthUI()
    {
        switch (playerHealth)
        {
            case 4:
                healthIndicator.sprite = null;
                healthIndicator.color = new Color(0, 0, 0, 0);
                break;
            case 3:
                healthIndicator.sprite = highDangerIndicator;
                healthIndicator.color = new Color(0, 0, 0, 0.5f);
                break;
            case 2:
                healthIndicator.sprite = dangerIndicator;
                healthIndicator.color = Color.white;
                break;
            case 1:
                healthIndicator.sprite = highDangerIndicator;
                healthIndicator.color = Color.white;
                break;
            default:
                break;
        }
    }

    //decreases the health of the player
    private void DecreaseHealth(int i)
    {
        //decrease the player health by however many is inputted
        if(playerHealth - i < 0)
        {
            playerHealth = 0;
        }
        else
        {
            playerHealth -= i;
        }

        //check for if the player is dead or not
        if (playerHealth <= 0)
        {
            //set isDead as true
            isDead = true;
        }
    }

    private void IncreaseHealth(int i)
    {
        if (playerHealth > 4)
        {
            playerHealth = i;
            return;
        }

        playerHealth += i;
    }

    private void JustHitCoolDown()
    {
        if (justHit && !prevJustHit)
        {
            //create a timestamp representing the end of the cooldown
            justHitTimeStamp = Time.time + justHitCoolDown;

            Debug.Log("timestamp for cooldown: " + justHitTimeStamp);
            prevJustHit = justHit;
        }

        //if the timestamp is less than or equal to the current time
        if (Time.time >= justHitTimeStamp)
        {
            //set justHit to false, allowing player to take damage again
            justHit = false;
            prevJustHit = justHit;
        }
    }

    private void HurtCoolDown()
    {
        if (hurt && playerHealth < 4 && !prevHurt)
        {
            if (playerHealth <= 1)
            {
                //create a timestamp representing the end of the cooldown
                //this is the closest to death so it will have a longer cooldown
                hurtTimeStamp = Time.time + highDangerCoolDown;
            }
            else
            {
                //create a timestamp representing the end of the cooldown
                hurtTimeStamp = Time.time + hurtCoolDown;

            }

            Debug.Log("timestamp for cooldown: " + justHitTimeStamp);
            prevHurt = hurt;
        }

        if (Time.time >= hurtTimeStamp && hurt)
        {
            IncreaseHealth(1);

            if(playerHealth >= 4) 
            {
                hurt = false;
                prevHurt = false;
            }
            else
            {
                //reset the timer
                hurtTimeStamp = Time.time + hurtCoolDown;
            }
        }

        if (!hurt)
        {
            prevHurt = false;
        }
    }

    //Player uses WASD to propel themselves faster, only while currently grabbing a bar
    private void PropelOffBar()
    {
        hasPropelled = false;
        //if the player is grabbing and no movement buttons are currently being pressed
        if (isGrabbing)
        {
            if (canPropel == false)
            {
                return;
            }
            // Check if no movement buttons are currently being pressed
            bool isThrusting = Mathf.Abs(thrust1D) > 0.1f;
            bool isStrafing = Mathf.Abs(strafe1D) > 0.1f;

            if (movementKeysReleased && (isThrusting || isStrafing))
            {
                //initialize a vector 3 for the propel direction
                hasPropelled = true;
                Vector3 propelDirection = Vector3.zero;

                //if W or S are pressed
                if (isThrusting)
                {
                    //release the bar and calculate the vector to propel based on the forward look
                    ReleaseBar();
                    propelDirection += cam.transform.forward * thrust1D * propelThrust;
                    //Debug.Log("Propelled forward or back");
                }
                //if A or D are pressed
                else if (isStrafing)
                {
                    //release the bar and calculate the vector to propel based on the right look
                    ReleaseBar();
                    propelDirection += cam.transform.right * strafe1D * propelThrust;
                    //Debug.Log("Propelled right or left");
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

        if (Physics.Raycast(ray, out hit, grabRange, barLayer | barrierLayer | doorLayer))
        {
            //Debug.Log("Hit: " + hit.transform.name + " | Tag: " + hit.transform.tag); // Debugging
            RayCastHandleGrab(hit);
            RayCastHandleDoorButton(hit);

            //if the current velocity is less than the parameter we set
            if(rb.velocity.magnitude <= pushSpeed)
            {
                //we handle interaction with pushing off the wall
                RayCastHandlePushOffWall(hit);
            }
        }
        else
        {
            ResetUI();
            potentialGrabbedBar = null;
            potentialWall = null;
        }
    }

    //helper methods for raycast handling
    public void RayCastHandleGrab(RaycastHit hit)
    {
        if (hit.transform.CompareTag("Grabbable"))
        {
            potentialGrabbedBar = hit.transform;
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

    public void RayCastHandlePushOffWall(RaycastHit hit)
    {
        //Debug.Log("Push off raycast happening");
        if(hit.transform.CompareTag("Barrier"))
        {
            potentialWall = hit.transform;
            //if in tutorial mode
            if (grabUIText.text == null && canPushOff)
            {
                //grabUIText.text = "'SPACEBAR'";
                //set the sprite for the space bar indicator
                inputIndicator.sprite = spaceIndicator;
                inputIndicator.color = new Color(256, 256, 256, 0.5f);
            }
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
        if(context.performed && potentialWall != null)
        {
            Debug.Log("space pressed");
            PropelOffWall();
        }
        else if (context.canceled)
        {
            DetectBarrierAndBounce();
        }
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
