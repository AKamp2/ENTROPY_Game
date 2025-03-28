using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    public ZeroGravity playerController;
    public DialogueManager dialogueManager;
    public DoorScript tutorialDoor;
    public PickupScript pickupObject;

    private int currentStep = 0;
    private bool isWaitingForAction = false;
    private Coroutine failureTimer;
    private bool tutorialSkipped = false;

    public AudioSource audioSource;
    public AudioClip jingle;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {

        dialogueManager.OnDialogueEnd += OnDialogueComplete;
        StartTutorial();
    }

    void Update()
    {
        // Skip tutorial with Enter
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            tutorialSkipped = true;
            EndTutorial();
        }

        if (isWaitingForAction)
        {
            if (currentStep == 1 && playerController.IsGrabbing)
            {
                CompleteStep();
            }
                
            else if (currentStep == 2 && playerController.HasPropelled)
            {
                CompleteStep();
                failureTimer = StartCoroutine(FailureCountdown(20f, 2)); // Retry step 2 on failure
            }
        }
    }

    void StartTutorial()
    {
        SetPlayerAbilities(false, false, false, false);
        dialogueManager.StartDialogueSequence(0, tutorialSkipped); // Ensure correct tutorial sequence index
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
                SetPlayerAbilities(true, false, false, false);
                dialogueManager.StartDialogueSequence(1, tutorialSkipped);
                isWaitingForAction = true;
                break;

            case 2:
                SetPlayerAbilities(true, true, false, false);
                dialogueManager.StartDialogueSequence(2, tutorialSkipped);
                isWaitingForAction = true;
                break;

            case 3:
                SetPlayerAbilities(true, true, true, true);
                isWaitingForAction = true;
                break;

            case 4:
                if (failureTimer != null) StopCoroutine(failureTimer);
                tutorialDoor.UnlockDoor();
                dialogueManager.StartDialogueSequence(3, tutorialSkipped);
                isWaitingForAction = true;
                break;

            case 5:
                dialogueManager.StartDialogueSequence(4, tutorialSkipped);
                isWaitingForAction = true;
                break;

            case 6:
                EndTutorial();
                break;
        }
    }

    public void CompleteStep()
    {
        isWaitingForAction = false;
        ProgressTutorial();
    }

    public void FailStep(int retryStep)
    {
        currentStep = retryStep;
        ProgressTutorial();
    }

    IEnumerator FailureCountdown(float time, int retryStep)
    {
        yield return new WaitForSeconds(time);
        if (isWaitingForAction && currentStep == retryStep)
        {
            FailStep(retryStep);
        }
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
        dialogueManager.StartDialogueSequence(5, tutorialSkipped);
        SetPlayerAbilities(true, true, true, true);
        isWaitingForAction = false;
        currentStep = 6;
    }

    private void OnDialogueComplete(int sequenceIndex)
    {
        if (!isWaitingForAction) return;
        CompleteStep();
    }
}


