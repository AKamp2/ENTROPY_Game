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
    public float fadeDuration = 1f;

    public AudioSource audioSource;
    public AudioClip jingle;

    float timer = 10f;
    bool firstFailurePlayed = false;

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
            tutorialDoor.LockDoor();
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
            EndTutorial();
        }

        if (isWaitingForAction)
        {
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
    }

    private IEnumerator StartTutorial()
    {
        SetPlayerAbilities(false, false, false, false);
        yield return new WaitForSeconds(1f);
        audioSource.PlayOneShot(jingle);
        dialogueManager.StartDialogueSequence(0); // Ensure correct tutorial sequence index
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
                Debug.Log("Tutorial 1");
                stepComplete = false;
                FadeIn(grabCanvasGroup);
                SetPlayerAbilities(true, false, false, false);
                isWaitingForAction = true;
                break;

            case 2:
                //player needs to propel from a bar
                Debug.Log("Tutorial 2");
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
                Debug.Log("Tutorial 5");
                stepComplete = false;
                FadeIn(throwItemCanvasGroup);
                isWaitingForAction = true;
                break;

            case 6:
                Debug.Log("Tutorial 6");
                EndTutorial();
                break;
        }
    }

    private IEnumerator WaitForBarGrab()
    {
        timer = 10f;  // Ensure timer is reset at the start

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
        Debug.Log(timer);
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
            Debug.Log("Failed to grab a bar in time.");
            if (firstFailurePlayed == false)
            {
                Debug.Log("Playing Failure Dialogue");
                FadeIn(pushOffCanvasGroup);
                StartCoroutine(dialogueManager.PlayFailureDialogue(0));
                firstFailurePlayed = true;
            }
            StartCoroutine(WaitForBarGrab());
        }

    }

    private IEnumerator WaitForSecondGrab()
    {
        timer = 10f;  // Reset the timer at the start of the coroutine
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
            tutorialDoor.UnlockDoor();
            CompleteStep();
        }
        else
        {
            if (!firstFailurePlayed)
            {
                Debug.Log("Play Failure Dialogue");
                firstFailurePlayed = true;
                yield return StartCoroutine(dialogueManager.PlayFailureDialogue(1));
            }

            //Debug.Log("Failure: Player must propel again.");
            yield return new WaitUntil(() => playerController.HasPropelled); // Wait for another propel attempt

            //Debug.Log("Player propelled again. Restarting grab check.");
            StartCoroutine(WaitForSecondGrab()); // Restart grab check AFTER propelling
        }
    }

    public void CompleteStep()
    {
        firstFailurePlayed = false;
        isWaitingForAction = false;
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
        endingDoor.UnlockDoor();
        currentStep = 6;
    }

    public bool TutorialStepCompleted()
    {
        return stepComplete;
    }

    private void OnDialogueComplete(int sequenceIndex)
    {
        if (!isWaitingForAction) return;
        CompleteStep();
    }

    private IEnumerator DelayTime(float delayTime)
    {
        yield return new WaitForSeconds(delayTime); // Wait for the specified time
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


