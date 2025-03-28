using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; } // Singleton instance

    // UI elements for dialogue display
    public DialogueSequence[] dialogueSequences;
    public Canvas dialogueCanvas;
    public TextMeshProUGUI nameTextUI;
    public TextMeshProUGUI dialogueTextUI;
    public AudioSource audioSource;
    public float typewriterSpeed = 0.08f;

    // Events for handling dialogue completion
    public event Action<int> OnDialogueEnd;
    private PlayerController playerController;
    public GameObject player; // Reference to player GameObject
    private ZeroGravity playerManager;

    // Tracking dialogue progress
    private int currentDialogueIndex = 0;
    private int currentSequenceIndex = -1;
    private bool isSkipping = false;
    private bool isDialogueActive = false;
    private bool tutorialSkipped = false;

    public bool IsDialogueActive => isDialogueActive; // Public access to dialogue state

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        playerController = new PlayerController();
    }

    private void OnEnable() => playerController.Dialogue.ContinueDialogue.Enable();
    private void OnDisable() => playerController.Dialogue.ContinueDialogue.Disable();

    private void Start()
    {
        playerManager = player.GetComponent<ZeroGravity>(); // Get reference to player movement manager
        dialogueCanvas.enabled = false; // Hide dialogue UI initially
    }

    private void Update()
    {
        // Allow skipping dialogue only if the player can move (not in a puzzle)
        if (playerManager.CanMove && playerController.Dialogue.ContinueDialogue.triggered)
        {
            isSkipping = true;
        }
    }

    /// <summary>
    /// Starts a dialogue sequence based on the given index.
    /// </summary>
    public void StartDialogueSequence(int sequenceIndex, bool tutorialSkipped = false)
    {
        if (sequenceIndex < dialogueSequences.Length)
        {
            this.tutorialSkipped = tutorialSkipped;
            currentSequenceIndex = sequenceIndex;
            currentDialogueIndex = 0;
            dialogueCanvas.enabled = true;
            isDialogueActive = true;
            StartCoroutine(DisplayDialogue());
        }
    }

    /// <summary>
    /// Handles displaying dialogue one line at a time.
    /// </summary>
    private IEnumerator DisplayDialogue()
    {
        DialogueSequence currentSequence = dialogueSequences[currentSequenceIndex];

        while (currentDialogueIndex < currentSequence.dialogues.Length)
        {
            Dialogue currentDialogue = currentSequence.dialogues[currentDialogueIndex];

            // Skip this dialogue if tutorial is skipped and this dialogue is marked to be skipped with the tutorial
            if (tutorialSkipped && currentDialogue.skipWithTutorial)
            {
                currentDialogueIndex++;
                continue;
            }

            nameTextUI.text = currentDialogue.characterName;

            // Play audio if available (one audio clip for multiple lines)
            if (currentDialogue.audioClip != null)
            {
                audioSource.clip = currentDialogue.audioClip;
                audioSource.Play();
            }

            //calculating dialogue speed
            int totalLength = 0;
            foreach (string line in currentDialogue.dialogueLines)
            {
                totalLength += line.Length;
            }

            // Adjust speed if an audio clip is present
            if (currentDialogue.audioClip != null)
            {
                typewriterSpeed = currentDialogue.audioClip.length / (float)totalLength - 0.01f;
            }

            // Show each line with typewriter effect, one by one
            foreach (string line in currentDialogue.dialogueLines)
            {
                yield return StartCoroutine(TypewriterEffect(line, currentDialogue.audioClip, typewriterSpeed));
                yield return new WaitForSeconds(0.5f);

            }

            // Wait until the audio clip finishes before moving to the next dialogue
            yield return new WaitUntil(() => !audioSource.isPlaying);
            yield return new WaitForSeconds(0.5f);

            currentDialogueIndex++;

        }

        // End dialogue
        isDialogueActive = false;
        dialogueCanvas.enabled = false;
        OnDialogueEnd?.Invoke(currentSequenceIndex);
    }

    /// <summary>
    /// Displays text with a typewriter effect.
    /// </summary>
    private IEnumerator TypewriterEffect(string dialogueText, AudioClip audioClip, float typewriterSpeed)
    {
        dialogueTextUI.text = "";
        isSkipping = false;

        

        foreach (char letter in dialogueText)
        {
            dialogueTextUI.text += letter;

            if (isSkipping)
            {
                dialogueTextUI.text = dialogueText;
                isSkipping = false;
                yield break;
            }

            yield return new WaitForSeconds(typewriterSpeed);
        }
    }
}