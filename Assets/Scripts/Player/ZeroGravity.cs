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

    [Header("== Player Movement Settings ==")]
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
    private bool justGrabbed = false;
    private bool prevJustGrabbed = false;

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
    private float minGrabRange = 1f;
    [SerializeField]
    private float grabPadding = 50f;
    //Propel off bar 
    [SerializeField]
    private float propelThrust = 50000f;

    [Header("== Push Off Wall Settings ==")]
    [SerializeField]
    private float propelOffWallThrust = 50000f;
    private Transform potentialWall = null;

    //fields for speed maxes for hit detection
    [SerializeField]
    private float pushSpeed = 1.5f;
    [SerializeField]
    private float dangerSpeed = 10f;
    [SerializeField]
    private float mediumSpeed = 5f;
    [SerializeField]
    private float zeroGWalkSpeed = 3f;
    [SerializeField]
    private float minimumSpeed = 1f;

    [Header("== Swinging Settings==")]
    private float maxSwingDistance = 0f; //max distance while swinging
    private float minSwingDistance = 0f; //minimum distance while swinging
    private Vector3 swingPoint; //stores the bar transform when calculating swings
    private SpringJoint joint;
    [SerializeField]
    private float grabDrag = .99f;

    //time stamps for swing cooldowns
    private float grabSwingTimeStamp;
    [SerializeField]
    private float swingCoolDownFastest = 4f;
    [SerializeField]
    private float swingCoolDownSlowest = 2f;


    [Header("== UI Settings ==")]
    [SerializeField]
    PlayerUIManager uiManager;
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
    private bool hasRolled = false;

    // Track if the movement keys were released
    private bool movementKeysReleased;

    #region properties
    //Properties
    //this property allows showTutorialMessages to be assigned outside of the script. Needed for the tutorial mission

    public float GrabPadding
    {
        get { return grabPadding; }
    }

    public bool ShowTutorialMessages
    {
        get { return showTutorialMessages; }
        set { showTutorialMessages = value; }
    }

    public Transform PotentialGrabbedBar
    {
        get { return potentialGrabbedBar; }
        set { potentialGrabbedBar = value; }
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
        get { return hasPropelled; }
        set { hasPropelled = value; }
    }

    public bool HasRolled
    {
        get { return hasRolled; }
        set { hasRolled = value; }
    }

    public int PlayerHealth { get { return playerHealth; } }

    public bool IsDead
    {
        get { return isDead; }
        set {  isDead = value; }
    }

    public float SensitivityX
    {
        get { return sensitivityX; }
        set { sensitivityX = value; }
    }

    public float SensitivityY
    {
        get { return sensitivityY; }
        set { sensitivityY = value; }
    }

    public float GrabRange
    {
        get { return grabRange; }
        set { grabRange = value; }
    }

    public LayerMask BarLayer
    {
        get { return barLayer; }
    } 

    // getter for isGrabbing
    public bool IsGrabbing => isGrabbing;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;  // Match this with your build target frame rate.

        //initial player booleans set if in tutorial mode
        if (tutorialMode)
        {
            canGrab = false;
            canPushOff = false;
            canPropel = false;
            canRoll = false;
        }
        //not in tutorial mode
        else
        {
            canGrab = true;
            canPushOff = true;
            canPropel = true;
            canRoll = true;
        }

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
                HandleRaycast();
                //handle grabber icon logic
                if (isGrabbing && grabbedBar != null && canGrab)
                {
                    if (canGrab)
                    {
                        //handle the grab movement
                        HandleGrabMovement(grabbedBar);
                        //keep grabber locked to grabbed bar
                        uiManager.UpdateGrabberPosition(grabbedBar);
                    }
                    //grabUIText.text = "'W A S D'";
                    //set the sprite for input indicator to the wasd indicator
                    if (canPropel) // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                    {
                        //inputIndicator.sprite = wasdIndicator;
                        //inputIndicator.color = new Color(256, 256, 256, 0.5f);
                    }

                }
                else
                {
                    //update to closest bar in view 
                    //UpdateClosestBarInView(); <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                }
            }
            else
            {
                //Debug.Log("Tutorial Mode off");
                HandleRaycast();
                //handle grabber icon logic
                if (isGrabbing && grabbedBar != null)
                {
                    //handle the grab movement
                    HandleGrabMovement(grabbedBar);
                }
                else
                {
                    //update to closest bar in view 
                    uiManager.UpdateClosestBarInView(); 
                }
            }
            //allow the player to bounce off the barriers
            DetectBarrierAndBounce();
            //take damage from door closing on the player
            DetectClosingDoorTakeDamageAndBounce();
            //track the player health and update the ui based on what health the player is on
            //uiManager.HandleHealthUI();

            //manage the cooldowns
            HurtCoolDown();
            JustHitCoolDown();
        }
    }

    void Update()
    {
        if (canMove)
        {
            RotateCam();
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
        if(rb.linearVelocity.magnitude <= pushSpeed && canPushOff)
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
        float ogSpeed = rb.linearVelocity.magnitude; //store initial velocity magnitude

        //Debug.Log(ogSpeed);

        foreach (Collider barrier in hitColliders)
        {
            Vector3 closestPoint = barrier.ClosestPoint(transform.position);
            Vector3 wallNormal = (transform.position - closestPoint).normalized;
            Vector3 reflectDirection = Vector3.Reflect(rb.linearVelocity.normalized, wallNormal);

            //push the player away slighly so they won't be stuck colliding with the wall.
            Vector3 pushAway = wallNormal * 0.005f; // small offset to prevent overlap
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
            avgBounceDirection.Normalize(); // average direction of the bounce
            float bounceSpeed = ogSpeed * .75f; // keep 75% of initial speed so it doesn't gain velocity on bounces

            //Debug.Log("BounceSpeed: " + bounceSpeed);

            //we want to make sure that all bounces off the wall while traversing are set to  minimum speed
            //however, if the player is grabbing we dont want these reset bounces as they jolt the player while grabbing
            if (!isGrabbing)
            {
                //verify the bounce is faster than minimum speed
                if (bounceSpeed <= minimumSpeed)
                {
                    //reset the bounce speed to be the minimum, ensuring constant bounces
                    bounceSpeed = minimumSpeed;
                }
            }

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.linearVelocity = avgBounceDirection * bounceSpeed;
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
        float ogSpeed = rb.linearVelocity.magnitude; //store initial velocity magnitude

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
                    Debug.Log("bounce count" + bounceCount);


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
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            //only normalize if avgBounceDirection is valid
            if (avgBounceDirection != Vector3.zero)
            {
                avgBounceDirection.Normalize();
            }

            //Debug.Log("Avg Bounce Direction: " + avgBounceDirection);

            float bounceSpeed = ogSpeed * .3f; // keep 30% of initial speed so it doesn't gain 

            //calculate the direction of the bounce
            Vector3 propelDirection = avgBounceDirection * ogSpeed * (propelThrust * .50f) * 0.05f;
            //Debug.Log("propel direction: " + propelDirection);
            rb.AddForce(propelDirection, ForceMode.VelocityChange);

            if (!isDead)
            {
                //decrease the player health after they have collided with the closing door
                isDead = true;
            }
        }
    }

    /// <summary>
    /// Simple method that only allows player to propel off a bar if they are currently grabbing it
    /// </summary>
    /// <param name="horizontalAxisPos"></param>
    /// <param name="verticalAxisPos"></param>
    private void HandleGrabMovement(Transform bar)
    {
        //Propel off bar logic
        if (isGrabbing && bar != null)
        {
            currentRollSpeed = 0.0f;
            PropelOffBar();
            Swing(bar);
            SwingCoolDown();
            uiManager.UpdateGrabberPosition(bar);
        }
    }

    private void HandleDoorInteraction(RaycastHit hit)
    {

        if (hit.transform.gameObject.CompareTag("DoorButton"))
        {
            //store the gameobject of the detected item and store it
            GameObject door = hit.transform.parent.gameObject;
            DoorScript ds = door.GetComponent<DoorScript>();


            if ((ds.DoorState == DoorScript.States.Open || ds.DoorState == DoorScript.States.Closed) && grabUIText.text == null)
            {
                //set the selected door in the door manager as this door
                doorManager.CurrentSelectedDoor = door;
                uiManager.DoorUI();
            }
        }
    }

    public void GrabBar()
    {
        isGrabbing = true;
        //set just grabbed to true to send to swing cool down
        justGrabbed = true;
        grabbedBar = potentialGrabbedBar;

        //lock grabbed bar and change icon
        uiManager.ShowGrabber(grabbedBar);
    }


    /// <summary>
    /// This method is created to allow player to swing on the bars, similar to a grappling hook feature found in other games
    /// It creates a sringjoint between the player and the bar the length of the players arm
    /// </summary>
    /// <param name="bar"></param>
    private void Swing(Transform bar)
    {
        if (isGrabbing && bar != null)
        {
            //Debug.Log("swingaling");
            swingPoint = bar.position;

            //ensure that the player isn't alr swinging on another bar
            if (this.gameObject.GetComponent<SpringJoint>() == null)
            {
                joint = this.gameObject.AddComponent<SpringJoint>();
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = swingPoint;
                float distanceFromPoint = Vector3.Distance(cam.transform.position, swingPoint);

                //ensure the max and min distances are set properly
                joint.maxDistance = grabRange;
                joint.minDistance = minGrabRange;
            }

            //tweak these values for the spring for better pendulum values
            joint.spring = 4.5f; //higher pull and push of the spring
            joint.damper = 7f;
            joint.massScale = 4.5f;
        }
    }

    /// <summary>
    /// This method will take the joint created by the Swing method and will incrimentally 
    /// shrink it as the player continues to hold onto one bar. This will eventually get short enough to 
    /// where the player is able to stop themself, and then propel with no swing affecting their trajectory
    /// </summary>
    private void PullToBar()
    {
        //if the joint is a long distance between the player and the bar
        if (joint.maxDistance >= joint.minDistance)
        {
            //decrease the length of the joint
            joint.maxDistance -= 0.1f;
            //lessen the spring force of the joint 
            joint.spring -= 0.1f;
        }
        //increment down the linear and angular velocities so the player slows down
        if (rb != null && rb.linearVelocity.magnitude > 0.1f)
        {
            //decrease the velocity
            rb.linearVelocity *= grabDrag;
        }

        //Debug.Log("linear velocity: " + rb.linearVelocity.magnitude);
    }
    /// <summary>
    /// This method will control the logic for the cooldown of swinging before the player 
    /// automatically starts gtting pulled into the bar to stop their movement while still holding the bar
    /// </summary>
    private void SwingCoolDown()
    {
        //confirm I have just grabbed a bar
        if (isGrabbing && justGrabbed && !prevJustGrabbed)
        {
            //Debug.Log("Linear velocity: " + rb.linearVelocity.magnitude);
            //create time stamps based on how fast the player is moving
            //if the player is moving at the dangerous speed
            if(rb.linearVelocity.magnitude >= mediumSpeed)
            {
                //Debug.Log("Danger Speed Reached");
                //the cooldown for swinging will be higher
                grabSwingTimeStamp = Time.time + swingCoolDownFastest;
                Debug.Log("medium Time Stamp: " + grabSwingTimeStamp + "TimeStampCurrent: " + Time.time);
            }
            //if the player is moving at a slower speed
            else if(rb.linearVelocity.magnitude >= zeroGWalkSpeed)
            {
                //Debug.Log("Normal Speed Reached");
                //the cooldown will be slower
                grabSwingTimeStamp = Time.time + swingCoolDownSlowest;
                Debug.Log("walk Time Stamp: " + grabSwingTimeStamp + "TimeStampCurrent: " + Time.time);
            }
            //set the prev just grabbed bool to confirm we do this once 
            prevJustGrabbed = justGrabbed;
        }
        //if the time has now gone past the cooldown timestamp we created
        if(Time.time > grabSwingTimeStamp)
        {
            //Debug.Log("pulling to bar");
            PullToBar();
            justGrabbed = false;
            prevJustGrabbed = justGrabbed;
            grabSwingTimeStamp = 0f;
        }
    }

    /// <summary>
    /// This method stops the swinging by destroying the pringjoint and setting the swingpoint back to zero
    /// </summary>
    private void StopSwing()
    {
        //Debug.Log("no swingaling");
        swingPoint = Vector3.zero;
        Destroy(joint);
    }


    // Release the bar and enable movement again
    private void ReleaseBar()
    {
        //stop swinging off the bar
        StopSwing();

        //set isGrabbing to false
        isGrabbing = false;
        //set justgrabbed to false to send for the cool down to nullify
        justGrabbed = false;
        grabbedBar = null;

        //lock grabbed bar and change icon
        uiManager.ReleaseGrabber();

        ////resume dynamic bar detection
        uiManager.UpdateClosestBarInView();
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
            playerHealth = 4;
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

            //Debug.Log("timestamp for cooldown: " + justHitTimeStamp);
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
            if (playerHealth == 1)
            {
                //create a timestamp representing the end of the cooldown
                //this is the closest to death so it will have a longer cooldown
                hurtTimeStamp = Time.time + highDangerCoolDown;
            }
            else if(playerHealth > 1)
            {
                //create a timestamp representing the end of the cooldown
                //this is the higher healths so the cool down will be shorter
                hurtTimeStamp = Time.time + hurtCoolDown;
            }
            //Debug.Log("timestamp for cooldown: " + justHitTimeStamp);
            prevHurt = hurt;
        }

        if (Time.time >= hurtTimeStamp && hurt)
        {
            IncreaseHealth(1);

            if(playerHealth >= 4) 
            {
                playerHealth = maxHealth;
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

    //player uses WASD to propel themselves faster, only while currently grabbing a bar
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
            //check if no movement buttons are currently being pressed
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

    public void Respawn(GameObject? respawnOverride = null)
    {
        GameObject targetLoc = respawnOverride ?? respawnLoc;
        transform.position = targetLoc.transform.position;
        cam.transform.rotation = targetLoc.transform.rotation;
        isDead = false;
        playerHealth = maxHealth;

        //reset all actions
        canGrab = true;
        canMove = true;
        canPropel = true;
        canRoll = true;
        canPushOff = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; // Reset velocity to prevent unwanted movement
            rb.angularVelocity = Vector3.zero;
        }
    }

    /// <summary>
    /// This method conrols the logic handling all of the raycasts from the player to diffeent objects in the scene allowing for multiple movement/ interaction options
    /// </summary>
    private void HandleRaycast()
    {
        if (isGrabbing)
        {
            // remove space and f gui if player is grabbing
            if (uiManager.InputIndicator.sprite != null)
            {
                potentialWall = null;

                //erase the input indicator
                uiManager.InputIndicator.sprite = null;
               uiManager.InputIndicator.color = new Color(0, 0, 0, 0);
            }
            
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
            if(rb.linearVelocity.magnitude <= pushSpeed)
            {
                //we handle interaction with pushing off the wall
                RayCastHandlePushOffWall(hit);
            }
        }
        else
        {
            //reset ui elements
            uiManager.ResetUI();
            //set the potential grabbed bar to null
            potentialGrabbedBar = null;
            //set the potential wall to null
            potentialWall = null;


            //doorManager.CurrentSelectedDoor = null;
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
        HandleDoorInteraction(hit);
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
                uiManager.InputIndicator.sprite = uiManager.SpaceIndicator;
                uiManager.InputIndicator.color = new Color(256, 256, 256, 0.5f);
            }
        }
    }

    #endregion

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
        hasRolled = true;
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
