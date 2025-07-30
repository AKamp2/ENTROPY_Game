using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using NUnit.Framework;

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField]
    private ZeroGravity player;
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private PickupScript pickupScript;
    [SerializeField]
    private DoorManager doorManager;
    // eventually replace with gameplay manager
    [SerializeField]
    private LockdownEvent lockdownEvent;

    private bool barInRaycast;
    private bool barInPeripheral;

    [Header("== UI Canvas ==")]
    [SerializeField]
    private CameraFade cameraFade;
    [SerializeField]
    private TextMeshProUGUI grabUIText;


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

    [SerializeField]
    private LayerMask barLayer; // Set a specific layer containing bars to grab onto
    [SerializeField]
    private LayerMask barrierLayer; //set layer for barriers
    [SerializeField]
    private LayerMask doorLayer;
    [SerializeField]
    private LayerMask raycastLayer; // Layer for general raycast interactions
    //this list stores the tags of all Interactable items we will have in ENTROPY, this will hopefully future proof ui interaction 
    [SerializeField]
    private List<string> interactables = new List<string>() {}; //populated in inspector

    //optimizing

    //components to cache the grabber rect transform and crosshair
    private RectTransform grabberRectTransform;
    private RectTransform crosshairRectTransform;

    #region properties
    public bool BarInRaycast
    {
        get { return barInRaycast; }
    }
    public bool BarInPeripheral
    {
        get { return barInPeripheral; }
    }

    public bool UIHandled
    {
        get { return UIHandled; }
    }

    public UnityEngine.UI.Image InputIndicator
    {
        get { return inputIndicator; }
        set { inputIndicator = value; }
    }

    public UnityEngine.UI.Image Crosshair 
    { 
        get { return crosshair; } 
        set { crosshair = value; }
    }

    public Sprite WASDIndicator { get { return wasdIndicator; } }

    public Sprite SpaceIndicator { get { return spaceIndicator; } }

    public Sprite RightClickIndicator { get { return rightClickIndicator; } }

    public Sprite LeftClickIndicator { get {return leftClickIndicator; } }

    public Sprite KeyFIndicator { get { return keyFIndicator; } }

    public Sprite DangerIndicator { get { return dangerIndicator; } }

    public Sprite HighDangerIndicator {  get { return highDangerIndicator; } }

    public LayerMask BarLayer
    {
        get { return barLayer; }
    }

    public LayerMask BarrierLayer
    {
        get { return barrierLayer; }
    }

    public LayerMask DoorLayer
    {
        get { return doorLayer; }
    }
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //set the crosshair and grabber sprites accordingly;
        crosshair.sprite = crosshairIcon;
        //set bar in view intially as false
        barInRaycast = false;
        barInPeripheral = false;
        //erase the grabber
        grabber.sprite = null;
        grabber.color = new Color(0, 0, 0, 0); //transparent
        //erase the input indicator
        inputIndicator.sprite = null;
        inputIndicator.color = new Color(0, 0, 0, 0); // transparent
        //hide the health indicator
        healthIndicator.sprite = null;
        healthIndicator.color = new Color(0, 0, 0, 0); //transparent

        //cached so we don't need to constantly look for them all the time 
        grabberRectTransform = grabber.GetComponent<RectTransform>();
        crosshairRectTransform = crosshair.GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleHealthUI();
        if (!player.IsGrabbing)
        {
            //search for bars in the raycast
            HandleRaycastUI();
            //if there are no bars in the raycast 
            if (!BarInRaycast)
            {
                //then we search for closest bar in the peripheral
                player.PotentialGrabbedBar = UpdateClosestBarInView();
            }
            //if there are no bars in the raycast or peripheral
            if (!BarInPeripheral && !barInRaycast)
            {
                //hide the hand
                HideGrabber();
            }
        }
        grabberRectTransform = grabber.GetComponent<RectTransform>();
    }

    /// <summary>
    /// This method conrols the logic handling all of the raycasts from the player to diffeent objects in the scene allowing for multiple movement/ interaction options
    /// </summary>
    public void HandleRaycastUI()
    {
        if (Time.frameCount % 2 != 0) return; //skip every other frame
        //Debug.Log("handle raycast called");
        RaycastHit hit;
        //create a raycast that stores the most favorable hit object, this will ensure when a bar is on screen it is chosen
        RaycastHit? barHit = null;
        //stores if something labeled as "button, switch" or any other interactable is hit by a ray
        RaycastHit? interactableHit = null; 
        float bestHitDistance = float.MaxValue;
        //this is a raycast that stores a fall back to ensure we can fall back on a previous hit if need be
        RaycastHit? wallHit = null;

        Vector2 screenCenter = crosshairRectTransform.position;
        float padding = player.GrabPadding;

        // Define padded bounds
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        Vector2 paddedMin = new Vector2(screenCenter.x - player.GrabPadding, screenCenter.y - player.GrabPadding);
        Vector2 paddedMax = new Vector2(screenCenter.x + player.GrabPadding, screenCenter.y + player.GrabPadding);

        for (float x = paddedMin.x; x <= paddedMax.x; x += player.GrabPadding / 4f)
        {
            for (float y = paddedMin.y; y <= paddedMax.y; y += player.GrabPadding / 4f)
            {
                Ray ray = player.cam.ScreenPointToRay(new Vector3(x, y, 0));
                string tag = null;

                // if raycast hits a bar
                if (Physics.Raycast(ray, out hit, player.GrabRange, barLayer))
                {
                    tag = hit.transform.tag;
                    //if the ray hits a grabbable object
                    if (tag == "Grabbable")
                    {
                        //Debug.Log("hit bar");
                        //create a vector for the position of the bar on the screen
                        Vector2 hitScreenPoint = player.cam.WorldToScreenPoint(hit.point);
                        //calculate the distance from the center to that point
                        float distanceToCenter = Vector2.Distance(hitScreenPoint, screenCenter);
                        if (distanceToCenter < bestHitDistance)
                        {
                            barHit = hit;
                            bestHitDistance = distanceToCenter;
                        }
                    }
                }
                else if (Physics.Raycast(ray, out hit, player.GrabRange, barrierLayer))
                {
                    tag = hit.transform.tag;
                    if (tag == "Barrier")
                    {
                        //ensure we arent continuously updating wall detections. we only need one
                        if(wallHit == null)
                        {
                            //store the barrier as a fallback if we don't get a bar on screen
                            wallHit = hit;
                        }
                    }
                }
                else if (Physics.Raycast(ray, out hit, player.GrabRange, raycastLayer))
                {
                    tag = hit.transform.tag;
                    //Debug.Log(tag);
                    //if we have this tag in our list
                    for(int i = 0; i < interactables.Count; i++)
                    {
                        Debug.Log(interactables[i]);
                        if(tag == interactables[i])
                        {
                            Debug.Log("interactable hit verified");
                            interactableHit = hit;
                        } 
                    }
                }
            }
        }

        //Debug.Log(barHit.HasValue.ToString());

        //now we take all that ish and process them here to determine the methods that shall be show base on simple hierarchy
        //1. available bar grabs should always be shown 
        //2. push off wall indicators should only be shown when there are no present bars
        //2. interactable items should always be shown
        if (interactableHit != null)
        {
            string interactTag = interactableHit.Value.collider.tag;
            switch (interactTag)
            {
                case "DoorButton":
                    RayCastHandleDoorButton(interactableHit);
                    break;
                case "LockdownLever":
                    Debug.Log("LockdownLever detected");
                    RayCastHandleManualLockdown(interactableHit);
                    break;
                case "WristGrab":
                    //Debug.Log("WristMonitor Detected");
                    RayCastHandleManualLockdown(interactableHit);
                    break;
                default:
                    break;
            }
        }
        else if(interactableHit == null)
        {
            //Debug.Log("interactable null");
            HideObjectives();
        }

        if (barHit != null)
        {
            //Debug.Log("bar hit: " + barHit);
            RayCastHandleGrab(barHit);
            return;
        }
        else if (barHit == null)
        {
            barInRaycast = false;
        }

        if (wallHit != null && !barInPeripheral && !barInRaycast)
        {
            RayCastHandlePushOffWall(wallHit);
            return;
        }
    }

    //helper methods for raycast handling
    public void RayCastHandleGrab(RaycastHit? hit)
    {
        //Debug.Log("raycast called");
        barInRaycast = true;
        player.PotentialGrabbedBar = hit.Value.transform;
        
    }

    public void RayCastHandleDoorButton(RaycastHit? hit)
    {
        if (hit.Value.transform.gameObject.CompareTag("DoorButton"))
        {
            //store the gameobject of the detected item and store it
            GameObject door = hit.Value.transform.parent.gameObject;
            DoorScript ds = door.GetComponent<DoorScript>();

            if (ds.DoorState == DoorScript.States.Open)
            {
                //set the selected door in the door manager as this door
                doorManager.CurrentSelectedDoor = door;
                inputIndicator.sprite = keyFIndicator;
                grabUIText.text = "Close Door";
                inputIndicator.color = new Color(1f, 1f, 1f, 0.5f);
            }
            else if(ds.DoorState == DoorScript.States.Closed)
            {
                doorManager.CurrentSelectedDoor = door;
                inputIndicator.sprite = keyFIndicator;
                grabUIText.text = "Open Door";
                inputIndicator.color = new Color(1f, 1f, 1f, 0.5f);
            }
           
        }
        else
        {
            doorManager.CurrentSelectedDoor = null;
            inputIndicator.sprite = null;
            inputIndicator.color = new Color(0, 0, 0, 0);
            grabUIText.text = "";

        }
    }

    public void RayCastHandlePushOffWall(RaycastHit? hit)
    {
        if (rb.linearVelocity.magnitude <= player.PushSpeed)
        {
            //Debug.Log("Push off raycast happening");
            if (hit.Value.transform.CompareTag("Barrier") && !barInRaycast && !barInPeripheral)
            {
                player.PotentialWall = hit.Value.transform;
                //if in tutorial mode
                if (player.CanPushOff)
                {
                    //grabUIText.text = "'SPACEBAR'";
                    //set the sprite for the space bar indicator
                    inputIndicator.sprite = spaceIndicator;
                    inputIndicator.color = new Color(1f, 1f, 1f, 0.5f);
                }
            }
            else
            {
                HidePushIndicator();
            }
        }
    }

    public void RayCastHandleManualLockdown(RaycastHit? hit)
    {
        if (hit.Value.transform.CompareTag("LockdownLever") && lockdownEvent && lockdownEvent.IsActive)
        {
            lockdownEvent.CanPull = true;
            grabUIText.text = "Deactivate manual lockdown";
            inputIndicator.sprite = keyFIndicator;
            inputIndicator.color = new Color(1f, 1f, 1f, 0.5f);
        }
        else if (lockdownEvent)
        {
            lockdownEvent.CanPull = false;
        }

        if (hit.Value.transform.CompareTag("WristGrab") && lockdownEvent && lockdownEvent.IsGrabbable)
        {
            lockdownEvent.CanGrab = true;
            grabUIText.text = "Take wrist monitor";
            inputIndicator.sprite = keyFIndicator;
            inputIndicator.color = new Color(1f, 1f, 1f, 0.5f);
        }
        else if (lockdownEvent)
        {
            lockdownEvent.CanGrab = false;
        }
    }

    public void HideObjectives()
    {
        //erase the input indicator
        inputIndicator.sprite = null;
        inputIndicator.color = new Color(0, 0, 0, 0);
        grabUIText.text = "";
        /*doorManager.DoorUI.SetActive(false);*/
    }
    /// <summary>
    /// this method removes the grabber sprite from the screen. making sure there are no floating grabbers in the ui
    /// </summary>
    public void HideGrabber()
    {
        //Debug.Log("hide grabber called");
        grabber.sprite = null;
        grabber.color = new Color(0, 0, 0, 0); //transparent
        //set the bar in view bool as false
        player.PotentialGrabbedBar = null;
    }

    public void HidePushIndicator()
    {
        if (inputIndicator.sprite == spaceIndicator)
        {
            inputIndicator.sprite = null;
            inputIndicator.color = new Color(0, 0, 0, 0);
        }
    }

    public void ReleaseGrabber()
    {
        grabber.sprite = openHand;
        inputIndicator.sprite = null;
        inputIndicator.color = new Color(0, 0, 0, 0);
    }

    /// <summary>
    /// This method calculates the closest bar in the player's screen. it will return a bar 
    /// transform so we can calculate its position relative to what ever raycasts, to get 
    /// one singular update grabber position call per frame
    /// </summary>
    /// <returns></returns>
    public Transform UpdateClosestBarInView()
    {
        //Debug.Log("updatedclosestw executed");
        //check for all nearby bars to the player
        Collider[] nearbyBars = Physics.OverlapSphere(transform.position, player.GrabRange, barLayer);
        Collider[] nearbyObjects;

        // Only track floating objects if able to pick up object
        if (pickupScript.CanPickUp && !pickupScript.HeldObject)
        {
             nearbyObjects = Physics.OverlapSphere(transform.position, pickupScript.PickUpRange, pickupScript.ObjectLayer);
        }
        else
        {
            nearbyObjects = new Collider[0];
        }

        // merge bar and object arrays
        Collider[] totalNearby = nearbyBars.Concat(nearbyObjects).ToArray();

        //initialize a transform for the closest bar and distance to that bar
        Transform closestObject = null;
        float closestDistance = Mathf.Infinity;

        //check through each bar in our array
        foreach (Collider obj in totalNearby)
        {
            //set specifications for the viewport
            Vector3 viewportPoint = player.cam.WorldToViewportPoint(obj.transform.position);

            //check if the bar is in the viewport and in front of the player
            if (viewportPoint.z > 0 && viewportPoint.x >= 0 && viewportPoint.x <= 1 && viewportPoint.y >= 0 && viewportPoint.y <= 1)
            {
                float distanceToBar = Vector3.Distance(transform.position, obj.transform.position);
                if (distanceToBar < closestDistance)
                {
                    closestDistance = distanceToBar;
                    closestObject = obj.transform;
                }
            }
        }
        //the closest object is not null and the player is not currently grabbing
        if (closestObject != null && !player.IsGrabbing)
        {
            // ensure closest object is a bar
            if (closestObject.gameObject.CompareTag("Grabbable"))
            {
                //Debug.Log("closestbar called");
                //update the grabber if a new bar is detected
                if (player.PotentialGrabbedBar != closestObject)
                {
                    //the potential bar is now this bar in view
                    player.PotentialGrabbedBar = closestObject;
                    barInPeripheral = true;
                    //Debug.Log("update closest bar in view found a bar");

                    //if in tutorial mode
                    if (player.TutorialMode && player.CanGrab)
                    {
                        //grabUIText.text = "press and hold 'RIGHT MOUSE BUTTON'";
                        //set the sprite for the right click
                        //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                        inputIndicator.sprite = rightClickIndicator;
                        inputIndicator.color = new Color(1f, 1f, 1f, 0.5f);
                    }
                }
                //update the sprite for the grabber
                grabberRectTransform.localScale = new Vector3(1, 1, 1);
                return closestObject;
            }
        }
        HideGrabber();
        barInPeripheral = false;
        return null;
    }

    // this method will update the grabber icon's position based on the nearest grabbable object
    public void UpdateGrabberPosition(Transform bar)
    {
        if (bar == null)
        {
            return;
        }
        if (player.CanGrab)
        {
            //check if their is a bar in the viewport
            if (bar != null)
            {
                //Debug.Log("updateGrabberexecuted");
                //set the position of the bar as a screen point
                Vector3 screenPoint = player.cam.WorldToScreenPoint(bar.position);

                //ensure the grabber is on the screen
                if (screenPoint.z > 0 && 
                    screenPoint.x > 0 && screenPoint.x < Screen.width &&
                    screenPoint.y > 0 && screenPoint.y < Screen.height)
                {
                    //update grabber position
                    grabberRectTransform.position = screenPoint;

                    //set hand icon open if not grabbing
                    if (!player.IsGrabbing)
                    {
                        grabber.sprite = openHand;
                        grabber.color = Color.white;
                    }
                    //set closed hand icon if grabbing
                    else
                    {
                        grabber.sprite = closedHand;
                        grabber.color = Color.white;
                    }

                    HidePushIndicator();
                }
                //if the z position of the grabber is off screen
                else
                {
                    if (player.IsGrabbing)
                    {
                        //update grabber position
                        if (screenPoint.z < 0)
                        {
                            screenPoint *= -1;
                        }
                        //create the screen center so we can translate the grabber to the edge relative to its position behind us
                        Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0) / 2;

                        //make 00 center of the screen instead of bottom left
                        screenPoint -= screenCenter;

                        //find angle from center of screen to mouse position
                        //takes x and y and creates an angle between them
                        float angle = Mathf.Atan2(screenPoint.y, screenPoint.x);
                        //subtract by 90 degrees converted to radians
                        angle -= 90 * Mathf.Deg2Rad;

                        //create a cos and sin from our angle created
                        float cos = Mathf.Cos(angle);
                        float sin = -Mathf.Sin(angle);

                        //apply a translation to the screenpoint from center based on the angle we created
                        screenPoint = screenCenter + new Vector3(sin * 150, cos * 150, 0);

                        // y = mx + b format
                        float m = cos / sin;

                        Vector3 screenBounds = screenCenter * 0.9f;

                        //check up and down to see which boundary of the screen to place the marker on
                        if (cos > 0)
                        {
                            screenPoint = new Vector3(screenBounds.y / m, screenBounds.y, 0);
                        }
                        else
                        {
                            //down 
                            screenPoint = new Vector3(-screenBounds.y / m, -screenBounds.y, 0);
                        }
                        //if out of bounds, get point on appropriate side
                        if (screenPoint.x > screenBounds.x) //out of bounds, must be on the right
                        {
                            screenPoint = new Vector3(screenBounds.x, screenBounds.x * m, 0);
                        }
                        else if (screenPoint.x < -screenBounds.x) // out of bounds left
                        {
                            screenPoint = new Vector3(-screenBounds.x, -screenBounds.x * m, 0);
                        } //else in bounds

                        //remove coordinate translation
                        screenPoint += screenCenter;
                        grabberRectTransform.position = screenPoint;

                        //set hand icon open if not grabbing
                        if (!player.IsGrabbing)
                        {
                            grabber.sprite = openHand;
                            grabber.color = Color.white;
                        }
                        //set closed hand icon if grabbing
                        else if (player.IsGrabbing)
                        {
                            grabber.sprite = closedHand;
                            grabber.color = Color.white;
                        }

                        //grabber.transform.localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
                        HidePushIndicator();
                    }
                }
            }
            else if(bar == null)
            {
                //remove the grabber
                HideGrabber();
            }
        }
    }   

    //Health Methods
    //controls the UI for the Player Health
    public void HandleHealthUI()
    {
        switch (player.PlayerHealth)
        {
            case 4:
                healthIndicator.sprite = null;
                healthIndicator.color = new Color(0, 0, 0, 0);
                break;
            case 3:
                healthIndicator.sprite = dangerIndicator;
                healthIndicator.color = new Color(1f, 1f, 1f, 0.5f);
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

   public void DoorUI()
    {
        inputIndicator.sprite = keyFIndicator;
        inputIndicator.color = new Color(1f, 1f, 1f, 0.5f);
    }

    void OnDrawGizmos()
    {
        // Visualize the crosshair padding as a box in front of the camera
        if (player.cam != null && crosshairRectTransform != null)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, crosshairRectTransform.position);

            // Define padded bounds
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            Vector2 paddedMin = new Vector2(screenPoint.x - player.GrabPadding, screenPoint.y - player.GrabPadding);
            Vector2 paddedMax = new Vector2(screenPoint.x + player.GrabPadding, screenPoint.y + player.GrabPadding);

            // Draw a box at the grab range with padding
            Gizmos.color = Color.green;
            for (float x = paddedMin.x; x <= paddedMax.x; x += player.GrabPadding / 4f)
            {
                for (float y = paddedMin.y; y <= paddedMax.y; y += player.GrabPadding / 4f)
                {
                    Ray ray = player.cam.ScreenPointToRay(new Vector3(x, y, 0));
                    Gizmos.DrawRay(ray.origin, ray.direction * player.GrabRange);
                }
            }
        }
    }

}
