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
        if(playerController.TutorialMode == true)
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
            tutorialSkipped = true;
            dialogueManager.TutorialSkipped = true;
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

    private IEnumerator StartTutorial()
    {
        SetPlayerAbilities(false, false, false, false);
        audioSource.PlayOneShot(jingle);
        yield return new WaitForSeconds(1f);
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
                Debug.Log("Tutorial 1");
                SetPlayerAbilities(true, false, false, false);
                dialogueManager.StartDialogueSequence(1);
                isWaitingForAction = true;
                break;

            case 2:
                Debug.Log("Tutorial 2");
                SetPlayerAbilities(true, true, false, false);
                dialogueManager.StartDialogueSequence(2);
                isWaitingForAction = true;
                break;

            case 3:
                Debug.Log("Tutorial 3");
                SetPlayerAbilities(true, true, true, true);
                isWaitingForAction = true;
                break;

            case 4:
                Debug.Log("Tutorial 4");
                if (failureTimer != null) StopCoroutine(failureTimer);
                tutorialDoor.UnlockDoor();
                dialogueManager.StartDialogueSequence(3);
                isWaitingForAction = true;
                break;

            case 5:
                Debug.Log("Tutorial 5");
                dialogueManager.StartDialogueSequence(4);
                isWaitingForAction = true;
                break;

            case 6:
                Debug.Log("Tutorial 6");
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
        dialogueManager.StartDialogueSequence(5);
        SetPlayerAbilities(true, true, true, true);
        isWaitingForAction = false;
        currentStep = 6;
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
}


