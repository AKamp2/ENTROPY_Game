using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    // Dialogue sequences and UI references
    public DialogueSequence[] dialogueSequences;
    public Canvas dialogueCanvas;
    public TextMeshProUGUI nameTextUI;
    public TextMeshProUGUI dialogueTextUI;
    public AudioSource audioSource;
    public float typewriterSpeed = 0.08f;

    // Events and input control
    public event Action<int> OnDialogueEnd;
    private PlayerController playerController;
    public GameObject player; //need reference to player GameObject
    private ZeroGravity playerManager;

    // Dialogue sequence tracking
    private int currentDialogueIndex = 0;
    private int currentSequenceIndex = -1;
    private bool isSkipping = false;
    private bool isDialogueActive = false;

    public bool IsDialogueActive => isDialogueActive;

    // Initialization
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
        //get reference to playerManager
        playerManager = player.GetComponent<ZeroGravity>();
        // Hide dialogue canvas on start
        dialogueCanvas.enabled = false;
    }

    private void Update()
    {
        // Check for skip input each frame, only if not viewing puzzle (can move is true)
        if (playerManager.CanMove == true)
        {
            if (playerController.Dialogue.ContinueDialogue.triggered)
            {
                isSkipping = true;
            }
        }
        
    }

    /// <summary>
    /// Starts a dialogue sequence by index
    /// </summary>
    /// <param name="sequenceIndex">The sequence to start</param>
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
    /// Displays the dialogue sequence on the UI
    /// </summary>
    /// <returns>IEnumerator for coroutine control</returns>
    private IEnumerator DisplayDialogue()
    {
        DialogueSequence currentSequence = dialogueSequences[currentSequenceIndex];

        while (currentDialogueIndex < currentSequence.dialogues.Length)
        {
            // Get current dialogue and set name
            Dialogue currentDialogue = currentSequence.dialogues[currentDialogueIndex];
            nameTextUI.text = currentDialogue.characterName;

            // Play audio clip if available
            if (currentDialogue.audioClip != null)
            {
                audioSource.clip = currentDialogue.audioClip;
                audioSource.Play();
            }

            // Display dialogue with typewriter effect
            yield return StartCoroutine(TypewriterEffect(currentDialogue.dialogueText, currentDialogue.audioClip));

            // Grace period for skipping
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => playerController.Dialogue.ContinueDialogue.triggered);

            if (audioSource.isPlaying) audioSource.Stop();
            currentDialogueIndex++;
        }

        // Dialogue sequence complete
        isDialogueActive = false;
        dialogueCanvas.enabled = false;
        OnDialogueEnd?.Invoke(currentSequenceIndex);
    }

    /// <summary>
    /// Displays the dialogue text using a typewritter effect
    /// </summary>
    /// <param name="dialogueText">The text to display with the effect</param>
    /// <param name="audioClip">Audio clip to set typewritting speed</param>
    /// <returns>IEnumerator for coroutine control</returns>
    private IEnumerator TypewriterEffect(string dialogueText, AudioClip audioClip)
    {
        dialogueTextUI.text = "";
        isSkipping = false;

        // Determine typewriter speed based on audio clip length if available
        if (audioClip != null)
        {
            typewriterSpeed = audioClip.length / dialogueText.Length - 0.02f;
        }

        foreach (char letter in dialogueText.ToCharArray())
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
    /// Pauses the current dialogue, stopping any audio and disabling input.
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
    /// Restarts the current dialogue sequence from the last dialogue index.
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

// Example of starting a dialogue sequence outside of the DialogueManager script:
// DialogueManager.Instance?.StartDialogueSequence(int);