using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    public GameObject ZeroGPlayer;
    private ZeroGravity playerController;
    public DialogueManager dialogueManager;
    public DoorScript doorToOpen;
    //public DoorScript endingDoor;
    private PickupScript pickupObject;

    //keep track when inside of the tutorial
    public bool inTutorial = false;

    private int currentStep = 0;
    private bool isWaitingForAction = false;
    private Coroutine failureTimer;
    private bool tutorialSkipped = false;
    private bool stepComplete = false;

    //tutorial canvas groups
    public CanvasGroup grabCanvasGroup;
    public CanvasGroup propelCanvasGroup;
    //public CanvasGroup pushOffCanvasGroup;
    //public CanvasGroup throwItemCanvasGroup;
    public CanvasGroup rollQCanvasGroup;
    public CanvasGroup rollECanvasGroup;
    public CanvasGroup enterCanvasGroup;

    [SerializeField] private Slider rollProgressBar;
    [SerializeField] private float requiredRotation = 180f; // how much roll needed

    public float fadeDuration = 1f;

    float timer = 10f;
    // failure flags so each only plays once
    private bool hasPlayedPushOffFailure = false;
    private bool hasPlayedRollFailure = false;
    private bool rollPanelHidden = false;
    private bool pushOffPanelHidden = false;


    //intended tutorial abilities
    private bool canGrab = true;
    private bool canRoll = true;
    private bool canPushOff = true;
    private bool canThrow = true;
    private bool canPropel = true;
    private float playerGrabRange;

    private float initialRollZ;

    public DialogueAudio dialogueAudio;
    public AmbientController ambientController;
    private WristMonitor wristMonitor;

    // rolling threshold (in degrees) beyond which we consider �upside down�
    [SerializeField] private float rollAngleThreshold = 150f;

    
    //timer for checking if player is upside down
    private float upsideDownTimer = 0f;
    private const float upsideDownDuration = 3f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        wristMonitor = FindFirstObjectByType<WristMonitor>();
        playerController = ZeroGPlayer.GetComponent<ZeroGravity>();
        pickupObject = ZeroGPlayer.GetComponent<PickupScript>();
        playerGrabRange = playerController.GrabRange;

        if (playerController.TutorialMode == true)
        {
            dialogueManager.OnDialogueEnd += OnDialogueComplete;
            StartCoroutine(StartTutorial());
        }

    }

    void Update()
    {
        // Skip tutorial with Enter
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            if (!tutorialSkipped)
            {
                tutorialSkipped = true;
                dialogueManager.SkipTutorial();
                FadeOut(enterCanvasGroup);
                EndTutorial();
            }
            
        }

        if (isWaitingForAction)
        {
            if (currentStep == 1 && playerController.IsGrabbing)
            {
                stepComplete = true;
                FadeOut(grabCanvasGroup);
                CompleteStep();
            }
            //Debug.Log("Waiting for step " + currentStep);
            //Debug.Log("Current step complete: " + stepComplete);
            else if (currentStep == 2)
            {

                UpdateRollProgress();
                //float currentZ = playerController.cam.transform.eulerAngles.z;
                //Debug.Log(currentZ);

                // Compute delta roll from initial orientation
                //float delta = Mathf.DeltaAngle(initialRollZ, currentZ);

                //bool isUpsideDownLook = Mathf.Abs(delta) >= 170f && Mathf.Abs(delta) <= 190f;
                //Debug.Log("Tutorial Detected Rotation " + playerController.TotalRotation);
                bool isUpsideDown = playerController.TotalRotation >= requiredRotation;

                //Debug.Log("Has rolled? "+ playerController.HasRolled);
                if (isUpsideDown)
                {
                    //Debug.Log("Player rolled upside down");
                    playerController.StopRollingQuickly();
                    stepComplete = true;
                    FadeOut(rollQCanvasGroup);
                    CompleteStep();
                    playerController.TotalRotation = 0;
                    rollProgressBar.gameObject.SetActive(false); // hide when done
                }
                //else if (isUpsideDownLook && !playerController.HasRolled)
                //{
                //    if (!hasPlayedRollFailure)
                //    {
                //        StartCoroutine(dialogueManager.PlayFailureDialogue(1));
                //        hasPlayedRollFailure = true;
                //    }
                //}
            }
            else if (currentStep == 3)
            {
                UpdateRollProgress();
                //float currentZ = playerController.cam.transform.eulerAngles.z;

                //float delta = Mathf.DeltaAngle(initialRollZ, currentZ);

                //bool isUpright = Mathf.Abs(delta) <= 10f; // close to original orientation
                bool isUpright = playerController.TotalRotation <= requiredRotation;

                if (isUpright)
                {
                    //Debug.Log("Player rolled upright");
                    playerController.StopRollingQuickly();
                    stepComplete = true;
                    FadeOut(rollECanvasGroup);
                    CompleteStep();
                    playerController.TotalRotation = 0;
                    rollProgressBar.gameObject.SetActive(false); // hide when done
                }
            }
            
            else if (currentStep == 4 && playerController.HasPropelled)
            {
                //Debug.Log("Detected player propel");

                playerController.HasPropelled = false; // Reset to prevent multiple detections
                SetPlayerAbilities(true, true, true, true, true);
                StartCoroutine(WaitForSecondGrab());

            }
        }
    }

    private IEnumerator StartTutorial()
    {
        SetPlayerAbilities(false, false, false, false, false);
        playerController.GrabRange = 1f;
        inTutorial = true;
        yield return new WaitForSeconds(1f);
        dialogueAudio.PlayJingle();
        FadeIn(enterCanvasGroup);
        dialogueManager.StartDialogueSequence(0, 2f); // Ensure correct tutorial sequence index

        //fading out the tutorial skip panel
        StartCoroutine(DelayFadeOut(7, enterCanvasGroup));
    }

    public void ProgressTutorial()
    {
        if (isWaitingForAction) return;
        currentStep++;
        RunTutorialStep();
    }

    void RunTutorialStep()
    {
        switch (currentStep)
        {
            case 1:
                // Step 1: Grab a bar
                //Debug.Log("Tutorial 1: Grab bar");
                stepComplete = false;
                isWaitingForAction = true;
                SetPlayerAbilities(true, false, false, false, false); // Only allow roll
                FadeIn(grabCanvasGroup);

                break;

            case 2:
                // Step 2: Roll 180 degrees upside down
                //Debug.Log("Tutorial 2: Roll upside down");
                if (!rollProgressBar.gameObject.activeSelf)
                {
                    rollProgressBar.gameObject.SetActive(true);
                    rollProgressBar.value = 0f; // reset to empty immediately
                }
                    
                stepComplete = false;
                isWaitingForAction = true;
                SetPlayerAbilities(true, false, false, true, false); // grab and roll only
                initialRollZ = playerController.cam.transform.eulerAngles.z;
                FadeIn(rollQCanvasGroup);
                
                break;

            case 3:
                // Step 3: Roll back upright
                //Debug.Log("Tutorial 3: Roll upright");
                if (!rollProgressBar.gameObject.activeSelf)
                {
                    rollProgressBar.gameObject.SetActive(true);
                    rollProgressBar.value = 0f; // reset to empty immediately
                }
                requiredRotation = -180f;
                stepComplete = false;
                isWaitingForAction = true;
                SetPlayerAbilities(true, false, false, true, false); // grab and roll only
                initialRollZ = playerController.cam.transform.eulerAngles.z;
                FadeIn(rollECanvasGroup);
                
                break;

            case 4:
                // Step 4: Propel and grab another bar
                //Debug.Log("Tutorial 4: Propel and grab another bar");
                stepComplete = false;
                isWaitingForAction = true;
                playerController.GrabRange = playerGrabRange;
                SetPlayerAbilities(true, true, true, true, true); // Enable all
                FadeIn(propelCanvasGroup);
                break;
            case 5:
                //Debug.Log("Tutorial Complete");
                EndTutorial();
                break;
        }
    }

    //IEnumerator for section 4. After the player has pushed off a bar, they need to grab another bar within 10 seconds to complete this challenge
    private IEnumerator WaitForSecondGrab()
    {
        float timer = 0f;
        float maxTime = 6f;
        bool barGrabbed = false;

        while (timer < maxTime)
        {
            if (playerController.IsGrabbing)
            {
                barGrabbed = true;
                break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (!barGrabbed)
        {
            //Debug.Log("Player failed to grab in time");

            if (dialogueManager.IsFailureTriggered)
            {
                yield return new WaitUntil(() => !dialogueManager.IsFailureTriggered);
            }

            dialogueManager.IsFailureTriggered = true;

            dialogueManager.SkipNextDialogue = true; // <- Tell it to skip the success dialogue
            StartCoroutine(dialogueManager.PlayFailureDialogue(0));
        }

        FadeOut(propelCanvasGroup);
        yield return new WaitUntil(() => !dialogueManager.IsFailureTriggered);
        stepComplete = true;
        CompleteStep();
    }

    public void CompleteStep()
    {
        isWaitingForAction = false;
    }

    public void SetPlayerAbilities(bool canGrab, bool canPropel, bool canPushOff, bool canRoll, bool canThrow)
    {
        playerController.CanGrab = canGrab;
        playerController.CanPropel = canPropel;
        playerController.CanPushOff = canPushOff;
        playerController.CanRoll = canRoll;
        pickupObject.CanPickUp = canThrow;

        this.canGrab = canGrab;
        this.canPropel = canPropel;
        this.canPushOff = canPushOff;
        this.canRoll = canRoll;
        this.canThrow = canThrow;
    }

    public void SetAbilitiesToTutorial()
    {
        playerController.CanGrab = canGrab;
        playerController.CanPropel = canPropel;
        playerController.CanPushOff = canPushOff;
        playerController.CanRoll = canRoll;
        pickupObject.CanPickUp = canThrow;
    }

    void EndTutorial()
    {
        SetPlayerAbilities(true, true, true, true, true);
        inTutorial = false;
        isWaitingForAction = false;
        playerController.TutorialMode = false;
        playerController.GrabRange = playerGrabRange;
        if(doorToOpen!=null)
        {
            if(doorToOpen.DoorState != DoorScript.States.Open)
            {
                doorToOpen.SetState(DoorScript.States.Open);
            }
        }

        //remove all tutorial panels
        HideAllPanels();
        ambientController.Progress();
        wristMonitor.CompleteObjective();
        currentStep = 5;

        dialogueManager.StartDialogueSequence(1, 0.2f);
    }

    //checks to see if the tutorial step is complete
    public bool TutorialStepCompleted()
    {
        //Debug.Log("Step completed? " + stepComplete);
        return stepComplete;
    }

    private void OnDialogueComplete(int sequenceIndex)
    {
        if (!isWaitingForAction) return;
        CompleteStep();
    }

    private IEnumerator DelayFadeOut(float delayTime, CanvasGroup canvas)
    {
        yield return new WaitForSeconds(delayTime); // Wait for the specified time
        FadeOut(canvas);
    }

    // Fade in the UI element (make it visible)
    public void FadeIn(CanvasGroup groupToFade)
    {
        StartCoroutine(FadeCanvasGroup(groupToFade, groupToFade.alpha, 1f));
    }

    // Fade out the UI element (make it invisible)
    public void FadeOut(CanvasGroup groupToFade)
    {
        StartCoroutine(FadeCanvasGroup(groupToFade, groupToFade.alpha, 0f));
    }

    // Coroutine to fade the CanvasGroup over time
    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha)
    {
        float timeElapsed = 0f;

        while (timeElapsed < fadeDuration)
        {
            // Lerp alpha from start to end
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, timeElapsed / fadeDuration);
            timeElapsed += Time.deltaTime;
            yield return null; // Wait until the next frame
        }

        canvasGroup.alpha = endAlpha; // Ensure it's set to the final alpha
    }

    public void RestartTutorial()
    {
        //Debug.Log("Restarting tutorial...");

        StopAllCoroutines();

        // Reset tutorial state
        inTutorial = true;
        currentStep = 0;
        isWaitingForAction = false;
        stepComplete = false;
        tutorialSkipped = false;

        playerController.HasRolled = false;

        // Reset failure flags
        hasPlayedPushOffFailure = false;
        hasPlayedRollFailure = false;
        rollPanelHidden = false;
        pushOffPanelHidden = false;

        // Reset ability flags
        canGrab = true;
        canRoll = true;
        canPushOff = true;
        canThrow = true;
        canPropel = true;

        // Set player movement ability to NONE at the start
        SetPlayerAbilities(false, false, false, false, false);

        // Hide all tutorial canvas elements
        HideAllPanels();

        // Reset skip flags in DialogueManager
        dialogueManager.SkipNextDialogue = false;
        dialogueManager.IsFailureTriggered = false;
        dialogueManager.TutorialSkipped = false;

        // Reactivate the tutorial door if it was opened
        if (doorToOpen != null)
        {
            doorToOpen.LockDoor();
        }

        // Restart dialogue from the beginning of the tutorial sequence
        dialogueManager.RestartCurrentDialogue(2f);

        // (Optional) Play intro jingle again
        dialogueAudio.PlayJingle();

        // Re-show the tutorial skip panel
        FadeIn(enterCanvasGroup);
        StartCoroutine(DelayFadeOut(7f, enterCanvasGroup));
    }

    private void HideAllPanels()
    {
        if(enterCanvasGroup.alpha == 1)
        {
            FadeOut(enterCanvasGroup);
        }
        if(rollQCanvasGroup.alpha == 1)
        {
            FadeOut(rollQCanvasGroup);
        }
        if (rollECanvasGroup.alpha == 1)
        {
            FadeOut(rollECanvasGroup);
        }
        if (grabCanvasGroup.alpha == 1)
        {
            FadeOut(grabCanvasGroup);
        }
        if (propelCanvasGroup.alpha == 1)
        {
            FadeOut(propelCanvasGroup);
        }
    }

    private void UpdateRollProgress()
    {
        float progress = 0f;

        // Rolling right (positive TotalRotation)
        if (requiredRotation > 0)
        {
            progress = Mathf.Clamp01(playerController.TotalRotation / requiredRotation);
        }
        // Rolling left (negative TotalRotation)
        else if (requiredRotation < 0)
        {
            progress = Mathf.Clamp01(playerController.TotalRotation / requiredRotation);
        }
        // else progress = 0

        rollProgressBar.value = progress;
    }


}


