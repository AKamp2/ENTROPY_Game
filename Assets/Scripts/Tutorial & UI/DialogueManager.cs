using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; } // Singleton instance

    // UI elements for dialogue display
    public DialogueSequence[] dialogueSequences;
    public DialogueSequence failureDialogues;
    public Canvas dialogueCanvas;
    public CanvasGroup dialogueCanvasGroup;
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

    public TutorialManager tutorialManager;

    private float fadeDuration = 0.5f;

    [Header("Skip Settings")]
    public AudioSource sfxSource;         // assign in inspector
    public AudioClip skipSfxClip;        
    public float skipPauseDuration = 0.3f;

    public bool IsDialogueActive => isDialogueActive; // Public access to dialogue state

    public bool TutorialSkipped
    {
        get { return tutorialSkipped; }
        set { tutorialSkipped = value; }
    }

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
        dialogueCanvasGroup = dialogueCanvas.GetComponent<CanvasGroup>();
        dialogueCanvasGroup.alpha = 0;
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
            StartCoroutine(DelayTime(2f, sequenceIndex));
        }
    }

    /// <summary>
    /// Handles displaying dialogue one line at a time.
    /// </summary>
    private IEnumerator DisplayDialogue()
    {
        FadeIn();
        isSkipping = false;
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
                typewriterSpeed = currentDialogue.audioClip.length / (float)totalLength - 0.015f;
            }

            // Show lines
            foreach (string line in currentDialogue.dialogueLines)
            {
                yield return StartCoroutine(TypewriterEffect(line, currentDialogue.audioClip, typewriterSpeed));

                // Skip: break out of current dialogue block
                if (isSkipping)
                {
                    break;
                }

                yield return new WaitForSeconds(0.3f);
            }

            // Skip: clear audio, advance dialogue index, continue outer loop
            if (isSkipping)
            {
                isSkipping = false;

                if (audioSource.isPlaying)
                    audioSource.Stop();

                if (currentDialogue.advancesTutorial == false)
                {
                    dialogueTextUI.text = "";
                }
                

                // Still advance tutorial if this dialogue was supposed to
                if (currentDialogue.advancesTutorial)
                {
                    //set the text to the last line in the set instead of wiping it
                    dialogueTextUI.text = currentDialogue.dialogueLines[currentDialogue.dialogueLines.Length - 1];
                    tutorialManager.ProgressTutorial();
                    yield return new WaitUntil(() => tutorialManager.TutorialStepCompleted());
                }

                yield return new WaitForSeconds(0.3f);
                currentDialogueIndex++;
                continue;
            }

            // Wait until the audio clip finishes before moving to the next dialogue unless skipping
            yield return new WaitUntil(() => !audioSource.isPlaying);
            yield return new WaitForSeconds(currentDialogue.delayBetweenDialogues);


            //advance tutorial if the dialogue is intended to.
            if(currentDialogue.advancesTutorial)
            {
                tutorialManager.ProgressTutorial();
                yield return new WaitUntil(() => tutorialManager.TutorialStepCompleted()); // Ensure the tutorial step is completed before continuing


            }
            currentDialogueIndex++;

        }

        // End dialogue
        FadeOut();
        isDialogueActive = false;
        dialogueCanvas.enabled = false;
        OnDialogueEnd?.Invoke(currentSequenceIndex);
    }

    public IEnumerator PlayFailureDialogue(int index)
    {
        Dialogue currentDialogue = failureDialogues.dialogues[index];

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
            yield return new WaitForSeconds(0.3f);

        }

        // Wait until the audio clip finishes before moving to the next dialogue unless skipping
        yield return new WaitUntil(() => !audioSource.isPlaying);
        yield return new WaitForSeconds(currentDialogue.delayBetweenDialogues);

        //advance tutorial if the dialogue is intended to.
        if (currentDialogue.advancesTutorial)
        {
            tutorialManager.ProgressTutorial();
        }

    }

    /// <summary>
    /// Displays text with a typewriter effect.
    /// </summary>
    private IEnumerator TypewriterEffect(string dialogueText, AudioClip audioClip, float typewriterSpeed)
    {
        dialogueTextUI.text = "";
        isSkipping = false;  // reset skip flag for this line

        foreach (char letter in dialogueText)
        {
            dialogueTextUI.text += letter;

            if (isSkipping)
            {
                if (sfxSource && skipSfxClip)
                    sfxSource.PlayOneShot(skipSfxClip);

                if (audioSource.isPlaying)
                    audioSource.Stop();

                
                yield break;
            }

            yield return new WaitForSeconds(typewriterSpeed);
        }
    }

    private IEnumerator DelayTime(float delayTime, int sequenceIndex)
    {
        yield return new WaitForSeconds(delayTime); // Wait for the specified time
        currentSequenceIndex = sequenceIndex;
        currentDialogueIndex = 0;
        dialogueCanvas.enabled = true;
        isDialogueActive = true;
        StartCoroutine(DisplayDialogue());
    }

    // Fade in the UI element (make it visible)
    public void FadeIn()
    {
        StartCoroutine(FadeCanvasGroup(dialogueCanvasGroup, dialogueCanvasGroup.alpha, 1f));
    }

    // Fade out the UI element (make it invisible)
    public void FadeOut()
    {
        StartCoroutine(FadeCanvasGroup(dialogueCanvasGroup, dialogueCanvasGroup.alpha, 0f));
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