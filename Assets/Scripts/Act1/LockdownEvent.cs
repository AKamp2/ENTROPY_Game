using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class LockdownEvent : MonoBehaviour
{
    [SerializeField]
    private GameObject playerObject;
    private ZeroGravity player;
    [SerializeField]
    private Collider playerCollider;

    [SerializeField]
    private BoxCollider DoorTrigger;
    [SerializeField]
    private GameObject lever;
    [SerializeField]
    private GameObject wrist;
    [SerializeField]
    private DoorScript[] doors;
    [SerializeField]
    private DoorScript brokenDoor;

    [SerializeField]
    private DoorScript bodyDoor;


    [SerializeField]
    private Material leverMaterial;

    // lockdown bools
    private bool isActive;
    private bool canPull;

    // wrist monitor
    private bool canGrab;
    private bool isGrabbable;

    public GameplayBeatAudio audioManager;
    public AnimationCurve powerDowncurve;
    public AnimationCurve glitchCurve;
    public Light[] lights;

    private bool glitchLights = false;
    private bool poweringDown = false;
    public float powerDownDuration = 1f;
    public float glitchDuration = 1f; // How long one cycle of the curve takes
    public float intensityMultiplier = 1f;

    private float elapsedTime = 0f;



    public bool CanPull
    {
        get { return canPull; }
        set { canPull = value; }
    }

    public bool IsActive
    {
        get { return isActive; }
    }

    public bool CanGrab
    {
        get { return canGrab; }
        set { canGrab = value; }
    }

    public bool IsGrabbable
    {
        get { return isGrabbable; }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = playerObject.GetComponent<ZeroGravity>();

        DoorTrigger.enabled = false;
        // checks if player is currently hovering over lever
        canPull = false;
        // checks if system is able to be turned on
        isActive = true;

        canGrab = false;
        isGrabbable = true;

    }

    // Update is called once per frame
    void Update()
    {
        if(poweringDown)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / powerDownDuration;

            // Only apply intensity if still within duration
            if (t <= 1f)
            {
                float curveValue = glitchCurve.Evaluate(t);
                foreach (Light lightSource in lights)
                {
                    lightSource.intensity = curveValue * intensityMultiplier;
                }

            }
            else
            {
                // Glitch complete, turn off or reset as needed
                poweringDown = false;
            }
        }

        if(glitchLights)
        {

            elapsedTime += Time.deltaTime;
            float t = elapsedTime / glitchDuration;

            // Only apply intensity if still within duration
            if (t <= 1f)
            {
                float curveValue = glitchCurve.Evaluate(t);
                foreach(Light lightSource in lights)
                {
                    lightSource.intensity = curveValue * intensityMultiplier;
                }
                
            }
            else
            {
                // Glitch complete, turn off or reset as needed
                glitchLights = false;
            }
        }
    }

    public void OpenDoors()
    {
        
        foreach (DoorScript door in doors)
        {
            StartCoroutine(OpenDoorWithDelay(door));
        }
        StartCoroutine(WaitForBodyVisible());
    }

    //adding slight delay to door to prevent phasing.
    private IEnumerator OpenDoorWithDelay(DoorScript door)
    {
        float randomDelay = Random.Range(0f, 0.2f); // Adjust range if needed
        yield return new WaitForSeconds(randomDelay);
        door.DoorState = DoorScript.States.Opening;
    }

    private IEnumerator WaitForBodyVisible()
    {
        yield return new WaitForSeconds(3f);
        audioManager.playBodyStinger();

    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (canPull && isActive)
        {
            // open the broken door first
            brokenDoor.DoorState = DoorScript.States.Opening;
            lever.GetComponent<Renderer>().material = leverMaterial;

            DoorTrigger.enabled = true;
            isActive = false;

            // begin lighting and audio queues
            StartCoroutine(PlayLockdownFX());
            
        }

        if (canGrab && IsGrabbable)
        {
            player.AccessPermissions[0] = true;

            isGrabbable = false;

            wrist.SetActive(false);
        }
    }

    private IEnumerator PlayLockdownFX()
    {
        StartCoroutine(LockDoors());
        audioManager.playPowerCut();
        poweringDown = true;
        
        yield return new WaitForSeconds(6f);

        audioManager.playPowerOn();
        glitchLights = true;
        yield return new WaitUntil(() => !glitchLights);
        foreach(Light lightSource in lights)
        {
            lightSource.intensity = 80f;
        }
    }

    private IEnumerator LockDoors()
    {
        brokenDoor.DoorState = DoorScript.States.Closing;
        yield return new WaitForSeconds(15f);
        brokenDoor.UseDoor();
    }

}
