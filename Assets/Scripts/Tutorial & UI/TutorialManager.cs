using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;
    public ZeroGravity playerController; // Reference to the player's movement script
    public DialogueManager dialogueManager; // Reference to the dialogue system

    private int currentStep = 0; // Tracks current tutorial step
    private bool isWaitingForAction = false; // Prevents progression until action is completed

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
            StartTutorial(); // Begin the tutorial when the game starts
        }
        
    }

    // Starts the tutorial sequence
    void StartTutorial()
    {
        playerController.CanGrab = false; // Disable player movement initially
        playerController.CanPropel = false;

        dialogueManager.StartDialogueSequence(0); // Begin tutorial dialogue sequence
    }

    // Progresses to the next tutorial step
    public void ProgressTutorial()
    {
        if (isWaitingForAction) return; // Ensure the previous step is completed first
        currentStep++;
        RunTutorialStep();
    }

    // Executes the tutorial steps based on progression
    void RunTutorialStep()
    {
        switch (currentStep)
        {
            case 1:
                dialogueManager.StartDialogueSequence(1); // Instruct player to grab a bar
                isWaitingForAction = true;
                break;
            case 2:
                
                dialogueManager.StartDialogueSequence(2); // Explain propulsion
                isWaitingForAction = true;
                break;
            case 3:
                dialogueManager.StartDialogueSequence(3); // Guide player to the door
                isWaitingForAction = true;
                break;
            case 4:
                dialogueManager.StartDialogueSequence(4); // Introduce throwing objects
                isWaitingForAction = true;
                break;
            case 5:
                EndTutorial(); // Finish tutorial
                break;
        }
    }

    // Marks a tutorial step as completed and allows progression
    public void CompleteStep()
    {
        isWaitingForAction = false;
        ProgressTutorial();
    }


    // Ends the tutorial sequence
    void EndTutorial()
    {
        dialogueManager.StartDialogueSequence(5); // Play tutorial completion dialogue
    }
}


