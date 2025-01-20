using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class TutorialScript : MonoBehaviour
{
    // Tutorial UI references
    public Canvas tutorialCanvas;
    public TextMeshProUGUI tutorialText;

    // Input and dialogue references
    public InputActionAsset inputActionAsset;
    private InputActionMap inputActionMap;

    // Tutorial sequence tracking
    private int currentStepIndex = 0;
    public float initialDelay = 2f;
    public float delayBetweenSteps = 0.5f;

    // Tutorial steps and instructions
    private readonly List<TutorialStep> tutorialSteps = new List<TutorialStep>
    {
        new TutorialStep("Continue", "Press F to continue dialogue"),
        new TutorialStep("PanCamera", "Move cursor to rotate camera"),
        new TutorialStep("MoveForward", "Hold W to move forward"),
        new TutorialStep("Interact", "Talk to the terminal to begin (Temp: Press Z)")
    };

    private void Awake()
    {
        // Find the Tutorial action map
        inputActionMap = inputActionAsset.FindActionMap("Tutorial");
    }

    private void Start()
    {
        // Hide tutorial canvas and start tutorial sequence
        tutorialCanvas.enabled = false;
        StartCoroutine(TutorialSequence());
    }

    /// <summary>
    /// Manages the tutorial sequence
    /// </summary>
    /// <returns>IEnumerator for coroutine control</returns>
    private IEnumerator TutorialSequence()
    {
        yield return new WaitForSeconds(initialDelay);
        tutorialCanvas.enabled = true;

        while (currentStepIndex < 4)
        {
            var step = tutorialSteps[currentStepIndex];
            yield return StartCoroutine(ShowStepAndWaitForAction(step.instruction, step.actionName));
            currentStepIndex++;
            yield return new WaitForSeconds(delayBetweenSteps);
        }

        tutorialCanvas.enabled = false;
        yield return new WaitForSeconds(delayBetweenSteps * 2);
        DialogueManager.Instance?.StartDialogueSequence(0);
    }

    /// <summary>
    /// Displays an instruction and waits until the specified action is performed by the player.
    /// </summary>
    /// <param name="instruction">The instruction text to display</param>
    /// <param name="actionName">The action name to wait for</param>
    /// <returns>IEnumerator for coroutine control</returns>
    private IEnumerator ShowStepAndWaitForAction(string instruction, string actionName)
    {
        tutorialText.text = instruction;
        InputAction currentAction = inputActionMap.FindAction(actionName);

        if (currentAction == null)
        {
            yield break;
        }

        currentAction.Enable();
        bool actionTriggered = false;

        // create callback and wait until action is triggered
        void callback(InputAction.CallbackContext ctx) => actionTriggered = true;
        currentAction.performed += callback;
        yield return new WaitUntil(() => actionTriggered);

        // remove callback and disable action
        currentAction.performed -= callback;
        currentAction.Disable();
    }
}