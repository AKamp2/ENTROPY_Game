using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class Dialogue
{
    public string characterName;
    public string[] dialogueLines; // Array of dialogue lines
    public AudioClip audioClip;
    public bool advancesTutorial;
    public bool skipWithTutorial;
    public bool incrementsDialogue;
    public float delayBetweenDialogues = 0.5f;
}

[System.Serializable]
public class DialogueSequence
{
    public Dialogue[] dialogues;
}

struct TutorialStep
{
    public string actionName;
    public string instruction;

    public TutorialStep(string actionName, string instruction)
    {
        this.actionName = actionName;
        this.instruction = instruction;
    }
}

[System.Serializable]
public class TutorialTask
{
    public enum TaskType { InputAction, Grab, Talk }
    
    public string instructionText;
    public Sprite actionIcon;
    public TaskType taskType;
    public InputActionReference actionReference;
}
