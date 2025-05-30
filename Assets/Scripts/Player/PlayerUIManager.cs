using UnityEngine;

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField]
    ZeroGravity player;

    [Header("== UI Canvas ==")]
    [SerializeField]
    private CameraFade cameraFade;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
    }

    // Update is called once per frame
    void Update()
    {
        HandleHealthUI();
    }

    public void ResetUI()
    {
        //erase the grabber
        grabber.sprite = null;
        grabber.color = new Color(0, 0, 0, 0);
        //erase the input indicator
        inputIndicator.sprite = null;
        inputIndicator.color = new Color(0, 0, 0, 0);
        /*doorManager.DoorUI.SetActive(false);*/
    }

    //public void UpdateClosestBarInView()
    //{
    //    //check for all nearby bars to the player
    //    Collider[] nearbyBars = Physics.OverlapSphere(transform.position, player.GrabRange, player.BarLayer);
    //    //initialize a transform for the closest bar and distance to that bar
    //    Transform closestBar = null;
    //    float closestDistance = Mathf.Infinity;

    //    //check through each bar in our array
    //    foreach (Collider bar in nearbyBars)
    //    {
    //        //set specifications for the viewport
    //        Vector3 viewportPoint = player.cam.WorldToViewportPoint(bar.transform.position);

    //        //check if the bar is in the viewport and in front of the player
    //        if (viewportPoint.z > 0 && viewportPoint.x >= 0 && viewportPoint.x <= 1 && viewportPoint.y >= 0 && viewportPoint.y <= 1)
    //        {
    //            float distanceToBar = Vector3.Distance(transform.position, bar.transform.position);
    //            if (distanceToBar < closestDistance)
    //            {
    //                closestDistance = distanceToBar;
    //                closestBar = bar.transform;
    //            }
    //        }
    //    }

    //    if (closestBar != null)
    //    {
    //        //update the grabber if a new bar is detected
    //        if (player.PotentialGrabbedBar != closestBar)
    //        {
    //            //player.PotentialGrabbedBar = closestBar;
    //            //if in tutorial mode
    //            if (player.TutorialMode && player.CanGrab)
    //            {
    //                //grabUIText.text = "press and hold 'RIGHT MOUSE BUTTON'";
    //                //set the sprite for the right click
    //                //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
    //                inputIndicator.sprite = rightClickIndicator;
    //                inputIndicator.color = new Color(256, 256, 256, 0.5f);
    //            }
    //            //update the sprite for the grabber and its position
    //            UpdateGrabberPosition(closestBar);
    //        }
    //    }
    //    else
    //    {
    //        //hide grabber if no bar is in range
    //        HideGrabber();
    //    }
    //}

    public void UpdateClosestBarInView()
    {
        //check for all nearby bars to the player
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, player.GrabRange, player.BarLayer);
        //initialize a transform for the closest bar and distance to that bar
        Transform closestObject = null;
        float closestDistance = Mathf.Infinity;

        //check through each bar in our array
        foreach (Collider obj in nearbyObjects)
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

        if (closestObject != null)
        {
            // ensure closest object is a bar
            if (closestObject.gameObject.CompareTag("Grabbable"))
            {
                //update the grabber if a new bar is detected
                if (player.PotentialGrabbedBar != closestObject)
                {
                    player.PotentialGrabbedBar = closestObject;
                    //if in tutorial mode
                    if (player.TutorialMode && player.CanGrab)
                    {
                        //grabUIText.text = "press and hold 'RIGHT MOUSE BUTTON'";
                        //set the sprite for the right click
                        //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                        inputIndicator.sprite = rightClickIndicator;
                        inputIndicator.color = new Color(256, 256, 256, 0.5f);
                    }
                    //update the sprite for the grabber and its position
                    UpdateGrabberPosition(closestObject);
                }
            }
            else
            {
                UpdateGrabberPosition(closestObject);
            }
            
        }
        else
        {
            //hide grabber if no bar is in range
            HideGrabber();
        }
    }

    // this method will update the grabber icon's position based on the nearest grabbable object
    public void UpdateGrabberPosition(Transform bar)
    {
        if (player.CanGrab)
        {
            //check if their is a bar in the viewport
            if (bar != null)
            {
                //set the position of the bar as a screen point
                Vector3 screenPoint = player.cam.WorldToScreenPoint(bar.position);

                if (screenPoint.z > 0)
                {
                    //update grabber position
                    grabber.rectTransform.position = screenPoint;

                    //set hand icon open is not grabbing
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

    public void ShowGrabber(Transform grabbedBar)
    {
        UpdateGrabberPosition(grabbedBar);
        grabber.sprite = closedHand;
    }

    public void ReleaseGrabber()
    {
        grabber.sprite = openHand;
        inputIndicator.sprite = null;
        inputIndicator.color = new Color(0, 0, 0, 0);
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

   public void DoorUI()
    {
        inputIndicator.sprite = keyFIndicator;
        inputIndicator.color = new Color(256, 256, 256, 0.5f);
    }

    void OnDrawGizmos()
    {
        // Visualize the crosshair padding as a box in front of the camera
        if (player.cam != null)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, crosshair.rectTransform.position);

            // Define padded bounds
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            Vector2 paddedMin = new Vector2(screenPoint.x - player.GrabPadding, screenPoint.y - player.GrabPadding);
            Vector2 paddedMax = new Vector2(screenPoint.x + player.GrabPadding, screenPoint.y + player.GrabPadding);

            // Draw a box at the grab range with padding
            Gizmos.color = Color.green;
            for (float x = paddedMin.x; x <= paddedMax.x; x += player.GrabPadding / 2)
            {
                for (float y = paddedMin.y; y <= paddedMax.y; y += player.GrabPadding / 2)
                {
                    Ray ray = player.cam.ScreenPointToRay(new Vector3(x, y, 0));
                    Gizmos.DrawRay(ray.origin, ray.direction * player.GrabRange);
                }
            }
        }
    }

}
