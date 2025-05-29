using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    public ZeroGravity playerController;
    public DialogueManager dialogueManager;
    public DoorScript tutorialDoor;
    public DoorScript endingDoor;
    public PickupScript pickupObject;


    private int currentStep = 0;
    private bool isWaitingForAction = false;
    private Coroutine failureTimer;
    private bool tutorialSkipped = false;
    private bool stepComplete = false;

    //tutorial canvas groups
    public CanvasGroup grabCanvasGroup;
    public CanvasGroup propelCanvasGroup;
    public CanvasGroup pushOffCanvasGroup;
    public CanvasGroup throwItemCanvasGroup;
    public CanvasGroup rollCanvasGroup;
    public CanvasGroup enterCanvasGroup;
    
    public float fadeDuration = 1f;

    public AudioSource audioSource;
    public AudioClip jingle;

    float timer = 10f;
    // failure flags so each only plays once
    private bool hasPlayedPushOffFailure = false;
    private bool hasPlayedRollFailure = false;
    private bool rollPanelHidden = false;
    private bool pushOffPanelHidden = false;

    // which failure-dialogue index in your failureDialogues array?
    [SerializeField] private int pushOffFailureIndex = 0;
    [SerializeField] private int rollFailureIndex = 2;  // adjust to your array

    // rolling threshold (in degrees) beyond which we consider “upside down”
    [SerializeField] private float rollAngleThreshold = 150f;

    // timer for object-grab step
    private Coroutine objectGrabTimer;
    
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
        if (playerController.TutorialMode == true)
        {
            if(tutorialDoor != null)
            {
                tutorialDoor.LockDoor();
            }

            dialogueManager.OnDialogueEnd += OnDialogueComplete;
            StartCoroutine(StartTutorial());
        }

    }

    void Update()
    {
        // Skip tutorial with Enter
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            tutorialSkipped = true;
            dialogueManager.TutorialSkipped = true;
            FadeOut(enterCanvasGroup);
            EndTutorial();
        }

        if (isWaitingForAction)
        {
            Debug.Log("Waiting for step " + currentStep);

            if (currentStep == 1 && playerController.IsGrabbing)
            {
                stepComplete = true;
                FadeOut(grabCanvasGroup);
                CompleteStep();
            }

            else if (currentStep == 2 && playerController.HasPropelled)
            {
                Debug.Log("Detected player propel");
                playerController.HasPropelled = false; // Reset to prevent multiple detections
                FadeOut(propelCanvasGroup);
                SetPlayerAbilities(true, true, true, true);
                stepComplete = true;
                CompleteStep();
                
            }
            else if (currentStep == 4 && playerController.HasPropelled)
            {
                Debug.Log("Detected player propel");

                playerController.HasPropelled = false; // Reset to prevent multiple detections
                SetPlayerAbilities(true, true, true, true);
                StartCoroutine(WaitForSecondGrab());

            }
            else if(currentStep == 5 && pickupObject.hasThrownObject)
            {
                FadeOut(throwItemCanvasGroup);
                stepComplete = true;
                CompleteStep();
            }
        }

        // Outside of steps 4 & 5, but only once per tutorial
        if (!hasPlayedRollFailure && playerController.CanRoll)
        {
            float zAngle = playerController.cam.transform.eulerAngles.z;
            if (zAngle > 180f) zAngle = 360f - zAngle;

            bool isUpsideDown = zAngle > rollAngleThreshold;

            // Increment timer if upside down, else reset it
            if (isUpsideDown && !playerController.HasRolled)
            {
                upsideDownTimer += Time.deltaTime;

                if (upsideDownTimer >= upsideDownDuration)
                {
                    StartCoroutine(PlayRollDialogue());
                    hasPlayedRollFailure = true; // mark as shown
                }
            }
            else
            {
                upsideDownTimer = 0f; // reset timer if not upside down
            }
        }

        //showing and hiding the roll tutorial canvas

        //conditions: if we know a failure dialogue is playing and it is the roll failure dialogue (index is the same), and the roll tutorial is not being shown, and if the player doesn't know how to roll yet.
        if (dialogueManager.IsFailureSpeaking && dialogueManager.CurrentFailureIndex == rollFailureIndex && rollCanvasGroup.alpha == 0 && !playerController.HasRolled)
        {
            Debug.Log("Showing QE panel");
            FadeIn(rollCanvasGroup);
        }

        //hide the canvas if the player pressed the roll button while the roll panel is visible, and only do this once
        //or hide the canvas if the roll canvas group is still open and the failure dialogue is done speaking
        if ((playerController.HasRolled || !dialogueManager.IsFailureSpeaking) && rollCanvasGroup.alpha == 1 && rollPanelHidden == false)
        {
            Debug.Log("Hiding QE panel");
            rollPanelHidden = true;
            StartCoroutine(DelayFadeOut(2, rollCanvasGroup));
        }

        //fading out space tutorial canvas
        if(pushOffCanvasGroup.alpha == 1 && !pushOffPanelHidden)
        {
            pushOffPanelHidden = true;
            StartCoroutine(DelayFadeOut(7, pushOffCanvasGroup));
        } 
    }

    private IEnumerator StartTutorial()
    {
        SetPlayerAbilities(false, false, false, false);
        yield return new WaitForSeconds(1f);
        audioSource.PlayOneShot(jingle);
        FadeIn(enterCanvasGroup);
        dialogueManager.StartDialogueSequence(0); // Ensure correct tutorial sequence index

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
                //player needs to grab a bar
                Debug.Log("Tutorial 1: Grab bar");
                stepComplete = false;
                FadeIn(grabCanvasGroup);
                SetPlayerAbilities(true, false, false, false);
                isWaitingForAction = true;
                break;

            case 2:
                //player needs to propel from a bar
                Debug.Log("Tutorial 2: Propel from bar");
                stepComplete = false;
                FadeIn(propelCanvasGroup);
                SetPlayerAbilities(true, true, false, false);
                isWaitingForAction = true;
                break;

            case 3:
                //player needs to grab another bar
                Debug.Log("Tutorial 3: Grab another bar.");
                stepComplete = false;
                isWaitingForAction = true;

                // Start the first timer for grabbing a bar
                StartCoroutine(WaitForBarGrab());
                break;

            case 4:
                //player needs to propel from one bar to the other
                Debug.Log("Tutorial 4: Grab a bar and propel to another.");
                SetPlayerAbilities(true, true, true, true);
                stepComplete = false;
                isWaitingForAction = true;
                break;

            case 5:
                //Grab an object
                Debug.Log("Tutorial 5: Throw Item");
                stepComplete = false;
                FadeIn(throwItemCanvasGroup);
                isWaitingForAction = true;

                // also start a 10s timer for NOT grabbing the object
                objectGrabTimer = StartCoroutine(PushOffFailureCountdown());
                break;

            case 6:
                Debug.Log("Tutorial 6: End");
                EndTutorial();
                break;
        }
    }

    //waiting for the player to grab a bar in the level
    private IEnumerator WaitForBarGrab()
    {
        //Debug.Log("Wait for bar grab coroutine");
        timer = 10f;
        bool barGrabbed = false;
        while (timer > 0)
        {
            //Debug.Log(timer);
            if (playerController.IsGrabbing)
            {
                barGrabbed = true;
                break;
            }

            timer -= Time.deltaTime;
            yield return null;
        }

        if (barGrabbed)
        {
            Debug.Log("Bar grabbed! Now propel to another bar.");
            FadeOut(pushOffCanvasGroup);
            stepComplete = true;
            SetPlayerAbilities(true, false, false, false);
            CompleteStep();
        }
        else
        {
            // if still waiting and we haven't played it yet:
            if (!hasPlayedPushOffFailure)
            {
                Debug.Log("Push off failure message can play");

                //wait till a previous failure dialogue stops playing before you play this one
                if (dialogueManager.IsFailureTriggered)
                {
                    yield return new WaitUntil(() => !dialogueManager.IsFailureTriggered);
                }
                dialogueManager.IsFailureTriggered = true;
                hasPlayedPushOffFailure = true;
                StartCoroutine(dialogueManager.PlayFailureDialogue(pushOffFailureIndex));

                // show your push-off tutorial panel here (if you have one)
                FadeIn(pushOffCanvasGroup);

            }

            //reset and wait for another grab
            StartCoroutine(WaitForBarGrab());

        }
    }

    //IEnumerator for section 4. After the player has pushed off a bar, they need to grab another bar within 10 seconds to complete this challenge
    private IEnumerator WaitForSecondGrab()
    {
        Debug.Log("Wait for second grab called");
        timer = 10f;
        bool barGrabbed = false;

        while (timer > 0)
        {
            if (playerController.IsGrabbing)
            {
                barGrabbed = true;
                break;
            }
            timer -= Time.deltaTime;
            yield return null;
        }

        if (barGrabbed)
        {
            Debug.Log("Success! Moving to next tutorial step.");
            stepComplete = true;
            tutorialDoor?.UnlockDoor();
            CompleteStep();
        }
        else
        {
            // if still waiting and we haven't played it yet:
            if (!hasPlayedPushOffFailure)
            {
                Debug.Log("Push off failure message can play");

                //wait till a previous failure dialogue stops playing before you play this one
                if (dialogueManager.IsFailureTriggered)
                {
                    yield return new WaitUntil(() => !dialogueManager.IsFailureTriggered);
                }
                dialogueManager.IsFailureTriggered = true;
                hasPlayedPushOffFailure = true;
                StartCoroutine(dialogueManager.PlayFailureDialogue(pushOffFailureIndex));

                // show your push-off tutorial panel here (if you have one)
                yield return new WaitUntil(() => rollCanvasGroup.alpha == 0);
                FadeIn(pushOffCanvasGroup);

            }

            yield return new WaitUntil(() => playerController.HasPropelled);

            StartCoroutine(WaitForSecondGrab());
        }
    }

    private IEnumerator PushOffFailureCountdown()
    {
        float t = 10f;
        while (t > 0f && isWaitingForAction)
        {
            t -= Time.deltaTime;
            yield return null;
        }

        // if still waiting and we haven't played it yet:
        if (isWaitingForAction && !hasPlayedPushOffFailure)
        {
            hasPlayedPushOffFailure = true;
            // show your push-off tutorial panel here (if you have one)
            FadeIn(pushOffCanvasGroup);
            // play the failure dialogue once
            StartCoroutine(dialogueManager.PlayFailureDialogue(pushOffFailureIndex));
        }
    }

    private IEnumerator PlayRollDialogue()
    {
        hasPlayedRollFailure = true;
        //wait for another failure dialogue to finish
        if (dialogueManager.IsFailureTriggered)
        {
            yield return new WaitUntil(() => !dialogueManager.IsFailureTriggered);
        }

        Debug.Log("Playing roll dialogue");
        

        // play the roll-failure dialogue
        dialogueManager.IsFailureTriggered = true;
        StartCoroutine(dialogueManager.PlayFailureDialogue(rollFailureIndex));
    }

    public void CompleteStep()
    {
        isWaitingForAction = false;

        if (objectGrabTimer != null)
            StopCoroutine(objectGrabTimer);
    }

    void SetPlayerAbilities(bool canGrab, bool canPropel, bool canPushOff, bool canRoll)
    {
        playerController.CanGrab = canGrab;
        playerController.CanPropel = canPropel;
        playerController.CanPushOff = canPushOff;
        playerController.CanRoll = canRoll;
    }

    void EndTutorial()
    {
        SetPlayerAbilities(true, true, true, true);
        isWaitingForAction = false;
        playerController.TutorialMode = false;
        if (endingDoor != null)
        {
            endingDoor.UnlockDoor();
        }
        if(tutorialDoor!=null)
        {
            if(tutorialDoor.DoorState != DoorScript.States.Open)
            {
                tutorialDoor.UnlockDoor();
            }
        }

        currentStep = 6;
    }

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

    
}


