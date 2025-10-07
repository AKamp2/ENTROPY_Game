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
    private BoxCollider MusicTrigger;
    [SerializeField]
    private BoxCollider cuttingOutTrigger;

    [SerializeField]
    private GameObject lever;
    [SerializeField]
    private Light buttonLight;
    [SerializeField]
    private GameObject wrist;
    [SerializeField]
    private DoorScript[] doorsToOpen;
    [SerializeField]
    private DoorScript[] doorsToLock;

    [SerializeField]
    private DoorScript medDoor;
    [SerializeField]
    private DoorScript brokenDoor;
    [SerializeField]
    private DoorScript bodyDoor;
    [SerializeField]
    private DoorScript escapeDoor;

    private DialogueManager dialogueManager;

    public HazardLight[] hazardsToDisable;
    public AmbientController ambientController;

    [SerializeField]
    private GameObject auxLightObj;
    [SerializeField]
    private Light auxLight;

    [SerializeField]
    private Material leverMaterial;

    // lockdown bools
    private bool isActive;
    private bool canPull;

    // wrist monitor
    private bool canGrab;
    private bool isGrabbable;
    [SerializeField]
    private WristMonitor wristMonitor;

    public GameplayBeatAudio audioManager;
    public AnimationCurve powerDownCurve;
    public AnimationCurve glitchCurve;
    public Light[] lights;
    public Light[] softLights;
    public Color endColor;
    public Color endButtonColor;
    public Color endEmissionColor;

    private bool glitchLights = false;
    private bool poweringDown = false;
    public float powerDownDuration = 1f;
    public float glitchDuration = 1f; // How long one cycle of the curve takes
    public float intensityMultiplier = 1f;

    private float powerDownElapsedTime = 0f;
    private float glitchElapsedTime = 0f;

    [SerializeField]
    private GameObject grateToMove;
    [SerializeField]
    private GameObject grateMoveLocation;

    private Vector3 gratePos;
    private Vector3 grateMovePos;

    [SerializeField]
    private CanvasGroup wristMonitorTutorial;

    [SerializeField]
    private GameObject serverObj;
    [SerializeField]
    private Material serverMaterial;
    private Material serverMaterialInstance;
    private Color initialEmissionColor;

    private bool tutorialMonitorFaded = false;



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
        if(grateToMove != null)
        {
            gratePos = grateToMove.transform.position;
        }

        if (grateMoveLocation != null)
        {
            grateMovePos = grateMoveLocation.transform.position;
        }

        player = playerObject.GetComponent<ZeroGravity>();

        DoorTrigger.enabled = false;
        //MusicTrigger.enabled = false;
        cuttingOutTrigger.enabled = false;
        // checks if player is currently hovering over lever
        canPull = false;
        // checks if system is able to be turned on
        isActive = true;

        canGrab = false;
        isGrabbable = true;

        dialogueManager = FindFirstObjectByType<DialogueManager>();

        
        Renderer rend = serverObj.GetComponent<Renderer>();
        serverMaterialInstance = rend.materials[1]; // this clones the material at runtime
        initialEmissionColor = serverMaterialInstance.GetColor("_EmissionColor");

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

                // Updates wrist monitor objective 
                wristMonitor.CompleteObjective();
            }
        }
    }

    public void LockDoors()
    {
        foreach(DoorScript door in doorsToLock)
        {
            door.SetState(DoorScript.States.Locked);
        }
    }

    public void OpenDoors()
    {
        
        foreach (DoorScript door in doorsToOpen)
        {
            StartCoroutine(OpenDoorWithDelay(door));
        }

        //deadBody.AddTorque(deadBody.transform.right * 15);
        //deadBody.AddForce(new Vector3(0, -1, 0) * 30);

        //StartCoroutine(WaitForBodyVisible());

        
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
        //Handle lockdown lever
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
            player.PlayerCutSceneHandler(true);
            StartCoroutine(PlayLockdownFX());
            
        }

        //Handle wrist monitor pickup
        if (canGrab && IsGrabbable)
        {
            player.AccessPermissions[0] = true;

            isGrabbable = false;

            audioManager.playMonitorPickup();

            wrist.SetActive(false);

            StartCoroutine(FadeCanvasGroup(wristMonitorTutorial, 0f, 1f));

            StartCoroutine(FadeTutorialPanelTimer());

            medDoor.SetState(DoorScript.States.Closed);

            ambientController.Progress();

            dialogueManager.StartDialogueSequence(6, 1f);

            cuttingOutTrigger.enabled = true;

            

        }
    }

    private IEnumerator PlayLockdownFX()
    {
        //Wait for button Press
        yield return new WaitForSeconds(0.5f);
        
        //Lock the Entrance Door;
        StartCoroutine(LockServerEntrance());
        MusicTrigger.enabled = true;

        foreach(HazardLight hazard in hazardsToDisable)
        {
            hazard.IsHazard = false;
        }

        bodyDoor.SetState(DoorScript.States.BrokenShort);
        LockDoors();

        audioManager.FadeServers(false);
        audioManager.playPowerCut();
        StartCoroutine(FadeEmission(serverMaterialInstance, initialEmissionColor, initialEmissionColor, 4f, 0, 6.5f, 0f));
        poweringDown = true;
        
        yield return new WaitForSeconds(7f);

        audioManager.playAlienRunAway();

        yield return new WaitForSeconds(7f);

        audioManager.playPowerOn();
        StartCoroutine(FadeEmission(serverMaterialInstance, initialEmissionColor, endEmissionColor, 0, 4f, 4f, 2f));
        glitchLights = true;
        foreach(Light light in lights)
        {
            StartCoroutine(FadeLightColor(light, light.color, endColor, 5f));
        }
        foreach (Light lightSource in softLights)
        {
            StartCoroutine(FadeLightIntensity(lightSource, 0.2f, 5f));
        }
        StartCoroutine(FadeLightIntensity(buttonLight, 0.5f, 5f));
        StartCoroutine(FadeLightColor(buttonLight, buttonLight.color, endButtonColor, 0.5f));
        
        yield return new WaitUntil(() => !glitchLights);
        foreach(Light lightSource in lights)
        {
            lightSource.intensity = intensityMultiplier;
        }
        
        yield return new WaitForSeconds(4f);
        player.PlayerCutSceneHandler(false);
        StartCoroutine(MoveDoor(gratePos, grateMovePos, 4f, null));
        dialogueManager.StartDialogueSequence(4, 0f);
        //Open doors in the doors to open array, this is the dining hall to facilities door.

        OpenDoors();
        escapeDoor.SetState(DoorScript.States.Open);
        audioManager.FadeServers(true);
        ambientController.Progress();
        
    }

    private IEnumerator LockServerEntrance()
    {
        brokenDoor.SetState(DoorScript.States.Closed);
        
        yield return new WaitForSeconds(19.5f);

        auxLightObj.GetComponent<Renderer>().material = leverMaterial;
        auxLight.color = endButtonColor;

        brokenDoor.SetState(DoorScript.States.Open);
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

    // Coroutine to fade the CanvasGroup over time
    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha)
    {
        float timeElapsed = 0f;
        float fadeDuration = 1f;

        while (timeElapsed < fadeDuration)
        {
            // Lerp alpha from start to end
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, timeElapsed / fadeDuration);
            timeElapsed += Time.deltaTime;
            yield return null; // Wait until the next frame
        }

        canvasGroup.alpha = endAlpha; // Ensure it's set to the final alpha
    }

    public IEnumerator FadeEmission(Material mat, Color fromColor, Color toColor, float fromIntensity, float toIntensity, float duration, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        // Ensure the emission is enabled on the material
        mat.EnableKeyword("_EMISSION");

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            // Interpolate color and intensity separately
            Color currentColor = Color.Lerp(fromColor, toColor, t);
            float currentIntensity = Mathf.Lerp(fromIntensity, toIntensity, t);

            // Apply combined color * intensity as emission
            mat.SetColor("_EmissionColor", currentColor * currentIntensity);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Final value at end of fade
        mat.SetColor("_EmissionColor", toColor * toIntensity);
    }

    public void FadeOutMonitorTutorial()
    {
        if(tutorialMonitorFaded == false)
        {
            tutorialMonitorFaded = true;
            StartCoroutine(FadeCanvasGroup(wristMonitorTutorial, 1f, 0f));
        }
        
    }

    private IEnumerator MoveDoor(Vector3 fromPos, Vector3 toPos, float duration, System.Action onComplete)
    {
        audioManager.PlayMoveGrate();
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = 0.5f * Mathf.Sin((elapsed / duration) * Mathf.PI - Mathf.PI / 2f) + 0.5f;
            grateToMove.transform.position = Vector3.Lerp(fromPos, toPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        grateToMove.transform.position = toPos;
        onComplete?.Invoke();
    }

    private IEnumerator FadeTutorialPanelTimer()
    {
        yield return new WaitForSeconds(8f);
        
        if(tutorialMonitorFaded == false)
        {
            tutorialMonitorFaded = true;
            StartCoroutine(FadeCanvasGroup(wristMonitorTutorial, 1f, 0f));
        }
    }

}
