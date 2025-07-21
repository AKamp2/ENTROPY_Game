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
    private Light buttonLight;
    [SerializeField]
    private GameObject wrist;
    [SerializeField]
    private DoorScript[] doors;

    [SerializeField]
    private DoorScript medDoor;
    [SerializeField]
    private DoorScript brokenDoor;
    [SerializeField]
    private DoorScript bodyDoor;
    [SerializeField]
    private Rigidbody deadBody;

    public DialogueManager dialogueManager;

    public HazardLight[] hazardsToDisable;

    [SerializeField]
    private Material leverMaterial;

    // lockdown bools
    private bool isActive;
    private bool canPull;

    // wrist monitor
    private bool canGrab;
    private bool isGrabbable;

    public GameplayBeatAudio audioManager;
    public AnimationCurve powerDownCurve;
    public AnimationCurve glitchCurve;
    public Light[] lights;
    public Color endColor;
    public Color endButtonColor;

    private bool glitchLights = false;
    private bool poweringDown = false;
    public float powerDownDuration = 1f;
    public float glitchDuration = 1f; // How long one cycle of the curve takes
    public float intensityMultiplier = 1f;

    private float powerDownElapsedTime = 0f;
    private float glitchElapsedTime = 0f;



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
            powerDownElapsedTime += Time.deltaTime;
            float t = powerDownElapsedTime / powerDownDuration;

            // Only apply intensity if still within duration
            if (t <= 1f)
            {
                float curveValue = powerDownCurve.Evaluate(t);
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

            glitchElapsedTime += Time.deltaTime;
            float t = glitchElapsedTime / glitchDuration;

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

        deadBody.AddTorque(deadBody.transform.right * 15);
        deadBody.AddForce(new Vector3(0, -1, 0) * 30);

        StartCoroutine(WaitForBodyVisible());

        
    }

    //adding slight delay to door to prevent phasing.
    private IEnumerator OpenDoorWithDelay(DoorScript door)
    {
        float randomDelay = Random.Range(0f, 0.2f); // Adjust range if needed
        yield return new WaitForSeconds(randomDelay);
        door.SetState(DoorScript.States.Open);
    }

    private IEnumerator WaitForBodyVisible()
    {
        yield return new WaitForSeconds(2f);
        audioManager.playBodyStinger();

    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (canPull && isActive)
        {
            audioManager.PlayButtonClick();
            buttonLight.intensity = 0;
            // open the broken door first
            brokenDoor.SetState(DoorScript.States.Open);
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

            audioManager.playMonitorPickup();

            wrist.SetActive(false);

            medDoor.SetState(DoorScript.States.Closed);


        }
    }

    private IEnumerator PlayLockdownFX()
    {
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(LockDoors());

        foreach(HazardLight hazard in hazardsToDisable)
        {
            hazard.IsHazard = false;
        }

        audioManager.FadeServers(false);
        audioManager.playPowerCut();
        poweringDown = true;
        
        yield return new WaitForSeconds(8.5f);

        audioManager.playPowerOn();
        glitchLights = true;
        foreach(Light light in lights)
        {
            StartCoroutine(FadeLightColor(light, light.color, endColor, 5f));
        }
        StartCoroutine(FadeLightIntensity(buttonLight, 0.5f, 5f));
        StartCoroutine(FadeLightColor(buttonLight, buttonLight.color, endButtonColor, 0.5f));
        
        yield return new WaitUntil(() => !glitchLights);
        foreach(Light lightSource in lights)
        {
            lightSource.intensity = intensityMultiplier;
        }
        yield return new WaitForSeconds(4f);
        dialogueManager.StartDialogueSequence(4, 0f);
        audioManager.FadeServers(true);
    }

    private IEnumerator LockDoors()
    {
        brokenDoor.SetState(DoorScript.States.Closed);
        yield return new WaitForSeconds(13f);
        brokenDoor.UseDoor();
    }

    public IEnumerator FadeLightColor(Light lightSource, Color fromColor, Color toColor, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            lightSource.color = Color.Lerp(fromColor, toColor, t);
            yield return null;
        }

        lightSource.color = toColor;
    }

    public IEnumerator FadeLightIntensity(Light lightSource, float targetIntensity, float duration)
    {
        float startIntensity = lightSource.intensity;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            lightSource.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
            yield return null;
        }

        lightSource.intensity = targetIntensity;
    }

}
