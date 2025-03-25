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

    public bool IsDialogueActive => isDialogueActive; // Public access to dialogue state

    // Ensures only one instance of DialogueManager exists
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
    public void StartDialogueSequence(int sequenceIndex)
    {
        if (sequenceIndex < dialogueSequences.Length)
        {
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
            nameTextUI.text = currentDialogue.characterName;

            // Play audio if available
            if (currentDialogue.audioClip != null)
            {
                audioSource.clip = currentDialogue.audioClip;
                audioSource.Play();
            }

            // Show text with typewriter effect
            yield return StartCoroutine(TypewriterEffect(currentDialogue.dialogueText, currentDialogue.audioClip));

            // Wait for player input to continue
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => playerController.Dialogue.ContinueDialogue.triggered);

            if (audioSource.isPlaying) audioSource.Stop();
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
    private IEnumerator TypewriterEffect(string dialogueText, AudioClip audioClip)
    {
        dialogueTextUI.text = "";
        isSkipping = false;

        // Adjust speed if an audio clip is present
        if (audioClip != null)
        {
            typewriterSpeed = audioClip.length / dialogueText.Length - 0.02f;
        }

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

    /// <summary>
    /// Pauses dialogue, disabling input and stopping audio.
    /// </summary>
    public void PauseDialogue()
    {
        if (isDialogueActive)
        {
            StopCoroutine(nameof(DisplayDialogue));
            audioSource.Pause();
            playerController.Dialogue.ContinueDialogue.Disable();
        }
    }

    /// <summary>
    /// Resumes dialogue from the last dialogue index.
    /// </summary>
    public void ResumeDialogue()
    {
        if (isDialogueActive)
        {
            StartCoroutine(DisplayDialogue());
            audioSource.UnPause();
            playerController.Dialogue.ContinueDialogue.Enable();
        }
    }
}