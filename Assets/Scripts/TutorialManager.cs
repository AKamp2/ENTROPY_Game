using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    public TutorialTask[] tutorialTasks;
    public TMP_Text instructionText;
    public Image actionImage;
    public Canvas tutorialCanvas;
    public CanvasGroup textCanvasGroup;
    public float fadeDuration = 0.5f;
    public float delayBetweenSteps = 0.5f;
    public GameObject player;
    private ZeroGravity playerManager;

    private int currentStep = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        playerManager = player.GetComponent<ZeroGravity>();
    }

    void Start()
    {
        tutorialCanvas.enabled = false;
        StartCoroutine(StartTutorial());
    }

    private IEnumerator StartTutorial()
    {
        yield return new WaitForSeconds(3f);
        tutorialCanvas.enabled = true;

        while (currentStep < tutorialTasks.Length)
        {
            yield return StartCoroutine(ShowStep(tutorialTasks[currentStep]));
            currentStep++;
            yield return new WaitForSeconds(delayBetweenSteps);
        }

        tutorialCanvas.enabled = false;
        yield return new WaitForSeconds(2f);

        DialogueManager.Instance?.StartDialogueSequence(0);
        Destroy(this);
    }

    private IEnumerator ShowStep(TutorialTask step)
    {
        // display instruction and image
        instructionText.text = step.instructionText;
        actionImage.sprite = step.actionIcon;

        // fade in
        yield return StartCoroutine(FadeCanvasGroup(textCanvasGroup, 0, 1, fadeDuration));

        // wait for task to be completed
        bool taskCompleted = false;
        step.actionReference?.action.Enable();

        while (!taskCompleted)
        {
            switch (step.taskType)
            {
                case TutorialTask.TaskType.InputAction:
                    taskCompleted = step.actionReference.action.triggered;
                    break;
                case TutorialTask.TaskType.Grab:
                    taskCompleted = playerManager != null && playerManager.IsGrabbing;
                    break;
                case TutorialTask.TaskType.Talk:
                    taskCompleted = CheckIfPlayerTalked();
                    break;
            }
            yield return null;
        }

        // disable action
        step.actionReference?.action.Disable();

        // fade out
        yield return StartCoroutine(FadeCanvasGroup(textCanvasGroup, 1, 0, fadeDuration));
    }

    private bool CheckIfPlayerTalked()
    {
        // Check for talk with Terminal to start story
        // auto start for now
        return true;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float start, float end, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(start, end, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = end;
    }
}
