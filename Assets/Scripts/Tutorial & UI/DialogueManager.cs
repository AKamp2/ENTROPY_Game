using System;
using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; } // Singleton instance

    // UI elements for dialogue display
    [Header("Dialogue Data (keep these compatible with your existing types)")]
    public DialogueSequence[] dialogueSequences;
    public DialogueSequence failureDialogues;


    [Header("UI")]
    public Canvas dialogueCanvas;
    public CanvasGroup dialogueCanvasGroup; // will be fetched in Awake if null
    public TextMeshProUGUI nameTextUI;
    public TextMeshProUGUI dialogueTextUI;

    [Header("Audio / Typewriter")]
    public AudioSource audioSource;
    public AudioClip fillerLineBeep;
    public float defaultTextSpeed = 0.08f;
    public DialogueAudio dialogueAudio; // kept for compatibility

    [Header("Fade / Timings")]
    public float fadeDuration = 0.5f;
    public float skipPauseDuration = 0.3f;

    [Header("Tutorial / External")]
    public TutorialManager tutorialManager;
    public GameObject player;
    public PlayerController playerController;
    public ZeroGravity playerManager;

    [Header("SFX")]
    public AudioSource sfxSource;
    public AudioClip skipSfxClip;

    // Public read-only state for external code compatibility
    public bool IsDialogueActive => currentState == DialogueState.Playing || currentState == DialogueState.Paused;
    public bool IsDialogueSpeaking => isLineTyping;
    public bool IsFailureSpeaking => isPlayingFailureDialogue;

    // Queue for requested sequences (preserves ordering)
    public readonly Queue<int> sequenceQueue = new Queue<int>();

    // Coroutine handles (tracked so we can stop them safely)
    private Coroutine displayCoroutine = null;
    private Coroutine fadeCoroutine = null;
    private Coroutine typewriterCoroutine = null;

    // Internal state
    private enum DialogueState { Idle, Playing, Paused, Fading }
    private DialogueState currentState = DialogueState.Idle;

    private bool isLineTyping = false;
    public bool isPlayingFailureDialogue = false;
    private bool playFillerBeep = false;
    private bool justBeeped = false;
    public bool skipRequested = false;

    // Indices that mirror your existing logic
    private int currentSequenceIndex = -1;
    private int currentDialogueIndex = 0;
    private int currentFailureIndex = -1;

    // Counts how many StartDialogueSequence calls are outstanding (kept for inspector/readout compatibility)
    public int numDialoguesQueued { get; private set; } = 0;

    public event Action<int> OnDialogueEnd;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (dialogueCanvasGroup == null && dialogueCanvas != null)
            dialogueCanvasGroup = dialogueCanvas.GetComponent<CanvasGroup>();

        // ensure starting visual state
        if (dialogueCanvas != null) dialogueCanvas.enabled = false;
        if (dialogueCanvasGroup != null) dialogueCanvasGroup.alpha = 0f;
        playerController = new PlayerController();
        ClearTextsImmediate();
    }

    private void OnEnable() => playerController.Dialogue.ContinueDialogue.Enable();
    private void OnDisable() => playerController.Dialogue.ContinueDialogue.Disable();

    private void Start()
    {
        if (player != null) playerManager = player.GetComponent<ZeroGravity>();
        // playerController can be wired in inspector; if not, don't assume new PlayerController()
    }

    private void Update()
    {
        // skip behavior: player presses continue to skip a line
        if (playerController.Dialogue.ContinueDialogue.triggered)
        {
            // request a skip; handled inside the typewriter/display flow
            skipRequested = true;
        }
    }

    #region Public API (kept shape similar to original)

    /// <summary>Enqueue a dialogue sequence. It will play after previously enqueued sequences finish.</summary>
    public void StartDialogueSequence(int sequenceIndex, float delayTime)
    {
        //account for index out of range

        if (sequenceIndex < 0 || sequenceIndex >= dialogueSequences.Length) return;
        sequenceQueue.Enqueue(sequenceIndex);
        numDialoguesQueued++;
        // If idle, kick off the queue processor
        if (currentState == DialogueState.Idle)
        {
            displayCoroutine = StartCoroutine(ProcessDialogueQueue());
        }
        // If already playing, the queued sequence will be handled in order
    }

    /// <summary>Play a failure dialogue immediately (pauses main flow cleanly).</summary>
    public void StartFailureDialogue(int failureIndex)
    {
        // fire-and-forget the failure coroutine; it will pause and then resume
        StartCoroutine(PlayFailureDialogueRoutine(failureIndex));
    }

    /// <summary>Force-stop everything and reset visual state. Use sparingly.</summary>
    public void ForceStopAll(bool clearQueue = true)
    {
        // stop tracked coroutines
        if (displayCoroutine != null) { StopCoroutine(displayCoroutine); displayCoroutine = null; }
        if (typewriterCoroutine != null) { StopCoroutine(typewriterCoroutine); typewriterCoroutine = null; }
        if (fadeCoroutine != null) { StopCoroutine(fadeCoroutine); fadeCoroutine = null; }

        currentState = DialogueState.Idle;
        isLineTyping = false;
        isPlayingFailureDialogue = false;
        skipRequested = false;
        sequenceQueue.Clear();
        if (clearQueue) numDialoguesQueued = 0;

        if (audioSource != null && audioSource.isPlaying) audioSource.Stop();
        ClearTextsImmediate();
        HideCanvasImmediate();
    }

    /// <summary>Restart the current sequence from its first dialogue.</summary>
    public void RestartCurrentDialogue(float delayTime)
    {
        // Stop only the display routine and typewriter so fade isn't clobbered
        if (displayCoroutine != null) { StopCoroutine(displayCoroutine); displayCoroutine = null; }
        if (typewriterCoroutine != null) { StopCoroutine(typewriterCoroutine); typewriterCoroutine = null; }

        isLineTyping = false;
        isPlayingFailureDialogue = false;
        skipRequested = false;

        // Reset indices
        currentDialogueIndex = 0;

        // Re-queue the same sequence to play again
        if (currentSequenceIndex >= 0 && currentSequenceIndex < dialogueSequences.Length)
        {
            sequenceQueue.Enqueue(currentSequenceIndex);
            numDialoguesQueued++;
            if (currentState == DialogueState.Idle)
                displayCoroutine = StartCoroutine(ProcessDialogueQueue(delayTime));
        }
        else
        {
            Debug.LogWarning("RestartCurrentDialogue: invalid currentSequenceIndex");
        }
    }

    #endregion

    #region Queue Processing & Display Flow

    private IEnumerator ProcessDialogueQueue(float startDelay = 0f)
    {
        // Slight optional delay before starting the first queued sequence
        if (startDelay > 0f) yield return new WaitForSeconds(startDelay);

        while (sequenceQueue.Count > 0)
        {
            int seqIndex = sequenceQueue.Dequeue();
            currentSequenceIndex = seqIndex;
            numDialoguesQueued = Mathf.Max(0, numDialoguesQueued - 1);

            // Play the sequence
            yield return StartCoroutine(PlaySequenceRoutine(seqIndex));

            // small heartbeat so other systems can catch up
            yield return null;
        }

        // queue drained
        displayCoroutine = null;
        currentState = DialogueState.Idle;
    }

    private IEnumerator PlaySequenceRoutine(int sequenceIndex)
    {
        if (sequenceIndex < 0 || sequenceIndex >= dialogueSequences.Length) yield break;
        DialogueSequence sequence = dialogueSequences[sequenceIndex];

        // initialize indices
        currentDialogueIndex = 0;

        // Ensure canvas visible and cleared before starting
        ClearDialogueTextBeforeShow();
        ShowCanvasImmediate(); // enables canvas
        yield return StartCoroutine(FadeCanvas(true));

        currentState = DialogueState.Playing;

        while (currentDialogueIndex < sequence.dialogues.Length)
        {
            // If paused by a failure, wait here until failure finishes.
            yield return new WaitWhile(() => currentState == DialogueState.Paused);

            Dialogue d = sequence.dialogues[currentDialogueIndex];

            // skip if tutorial global skip and this dialogue wanted to be skipped
            if (tutorialManager != null && tutorialManager.IsTutorialSkipped && d.skipWithTutorial && sequenceIndex == 0)
            {
                currentDialogueIndex++;
                continue;
            }

            // Set name and audio
            if (nameTextUI != null) nameTextUI.text = d.characterName ?? "";
            if (d.audioClip != null && audioSource != null)
            {
                audioSource.clip = d.audioClip;
                audioSource.Play();
                playFillerBeep = false;
            }
            else
            {
                playFillerBeep = true;
            }

            // compute typewriter speed
            float typeSpeed = defaultTextSpeed;
            if (d.audioClip != null && d.dialogueLines != null)
            {
                int totalLength = 0;
                foreach (var l in d.dialogueLines) totalLength += l.Length;
                if (totalLength > 0) typeSpeed = Mathf.Max(0.001f, d.audioClip.length / (float)totalLength - 0.01f);
            }

            // Show each line
            foreach (string line in d.dialogueLines)
            {
                // honor pause caused externally
                yield return new WaitWhile(() => currentState == DialogueState.Paused);

                // Typewriter: tracked coroutine so we can stop or interrupt it
                if (typewriterCoroutine != null) { StopCoroutine(typewriterCoroutine); typewriterCoroutine = null; }
                typewriterCoroutine = StartCoroutine(TypewriterEffect(line, typeSpeed));
                yield return typewriterCoroutine;
                typewriterCoroutine = null;

                // If skip was requested during line, consume skip and break out to next dialogue
                if (skipRequested)
                {
                    Debug.Log("Skip is being called");
                    skipRequested = false;
                    if (audioSource != null && audioSource.isPlaying) audioSource.Stop();

                    if (d.advancesTutorial == false)
                    {
                        dialogueTextUI.text = "";
                    }
                }

                // small buffer between lines
                yield return new WaitForSeconds(0.3f);
            }

            // if d advances tutorial, call progress and wait for completion
            if (d.advancesTutorial && tutorialManager != null)
            {
                tutorialManager.ProgressTutorial();
                yield return new WaitUntil(() => tutorialManager.TutorialStepCompleted());
            }

            // wait for dialogue clip to finish (if any) before continuing
            if (audioSource != null && audioSource.clip != null)
            {
                yield return new WaitWhile(() => audioSource.isPlaying);
            }

            // small post-dialogue delay
            yield return new WaitForSeconds(d.delayBetweenDialogues);

            currentDialogueIndex++;
        }

        // sequence finished
        // give OnDialogueEnd event similar to original behavior
        OnDialogueEndSafe(currentSequenceIndex);

        // fade out if no queued sequences are pending
        if (sequenceQueue.Count <= 0)
        {
            yield return StartCoroutine(FadeCanvas(false));
            HideCanvasImmediate();
        }
    }

    #endregion

    #region Failure Dialogue Handling

    public IEnumerator PlayFailureDialogueRoutine(int index)
    {
        if (failureDialogues == null || index < 0 || index >= failureDialogues.dialogues.Length) yield break;

        // Wait until any currently typing line finishes (so we don't cut mid-letter)
        yield return new WaitUntil(() => !isLineTyping);

        yield return new WaitUntil(() => isPlayingFailureDialogue == false);

        // Pause the main sequence without clearing its indices or state
        DialogueState prevState = currentState;
        currentState = DialogueState.Paused;
        isPlayingFailureDialogue = true;
        currentFailureIndex = index;

        // ensure canvas is visible
        ClearDialogueTextBeforeShow();
        ShowCanvasImmediate();
        yield return StartCoroutine(FadeCanvas(true));

        Dialogue d = failureDialogues.dialogues[index];

        if (nameTextUI != null) nameTextUI.text = d.characterName ?? "";
        if (d.audioClip != null && audioSource != null)
        {
            audioSource.clip = d.audioClip;
            audioSource.Play();
            playFillerBeep = false;
        }
        else
        {
            playFillerBeep = true;
        }

        // compute typewriter speed for failure dialogue
        float typeSpeed = defaultTextSpeed;
        if (d.audioClip != null && d.dialogueLines != null)
        {
            int totalLength = 0;
            foreach (var l in d.dialogueLines) totalLength += l.Length;
            if (totalLength > 0) typeSpeed = Mathf.Max(0.001f, d.audioClip.length / (float)totalLength - 0.01f);
        }

        // play lines
        foreach (string line in d.dialogueLines)
        {
            if (typewriterCoroutine != null) { StopCoroutine(typewriterCoroutine); typewriterCoroutine = null; }
            typewriterCoroutine = StartCoroutine(TypewriterEffect(line, typeSpeed));
            yield return typewriterCoroutine;
            typewriterCoroutine = null;

            yield return new WaitForSeconds(0.3f);
        }

        // wait for audio & delay
        if (audioSource != null && audioSource.clip != null) yield return new WaitWhile(() => audioSource.isPlaying);
        yield return new WaitForSeconds(d.delayBetweenDialogues);

        // failure-specific tutorial handling (keeps your original logic)
        if (d.advancesTutorial && tutorialManager != null && !tutorialManager.TutorialStepCompleted())
        {
            tutorialManager.CompleteStep();
            tutorialManager.ProgressTutorial();
            // do not block here — original code didn't wait; keep same behavior
        }

        // optionally increment the main dialogue index if this failure wanted to skip the next
        if (d.incrementsDialogue) currentDialogueIndex = Mathf.Min(currentDialogueIndex + 1, (dialogueSequences.Length > 0 && currentSequenceIndex >= 0 && currentSequenceIndex < dialogueSequences.Length) ? dialogueSequences[currentSequenceIndex].dialogues.Length : currentDialogueIndex);

        // clear failure flags and resume
        isPlayingFailureDialogue = false;
        currentFailureIndex = -1;
        currentState = prevState == DialogueState.Playing ? DialogueState.Playing : DialogueState.Idle;

        // If nothing is playing after the failure (no queued sequences), fade out and hide
        if (currentState == DialogueState.Idle && sequenceQueue.Count == 0)
        {
            yield return StartCoroutine(FadeCanvas(false));
            HideCanvasImmediate();
        }
    }

    #endregion

    #region Typewriter & Helpers

    private IEnumerator TypewriterEffect(string fullText, float charDelay)
    {
        isLineTyping = true;
        dialogueTextUI.text = "";

        justBeeped = false;
        

        for (int i = 0; i < fullText.Length; i++)
        {
            // If a failure paused us mid-type, break (failure routine will handle resume / retype if desired)
            if (currentState == DialogueState.Paused) break;

            dialogueTextUI.text += fullText[i];

            if (playFillerBeep && fillerLineBeep != null && audioSource != null)
            {
                // alternate beep on characters to avoid spam
                if (!justBeeped)
                {
                    audioSource.PlayOneShot(fillerLineBeep);
                    justBeeped = true;
                }
                else justBeeped = false;
            }

            // allow skipping the line
            if (skipRequested)
            {
                if (sfxSource && skipSfxClip)
                    sfxSource.PlayOneShot(skipSfxClip);

                if (audioSource.isPlaying)
                    audioSource.Stop();

                isLineTyping = false;
                yield break;
            }

            yield return new WaitForSeconds(charDelay);
        }

        isLineTyping = false;
    }

    /// <summary>
    /// Immediately skip the tutorial dialogue sequence.
    /// Stops all active tutorial dialogue, clears the queue, and fades out the canvas.
    /// </summary>
    public void SkipTutorial()
    {
        if (sfxSource && skipSfxClip) sfxSource.PlayOneShot(skipSfxClip);

        // Stop any active coroutines cleanly
        ForceStopAll(clearQueue: true);

        // Fade out the dialogue canvas visually (don’t rely on ForceStopAll hiding it instantly)
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeCanvas(false));

        //Debug.Log("Tutorial skipped by player.");
    }


    private IEnumerator FadeCanvas(bool fadeIn)
    {
        // stop any existing fade
        if (fadeCoroutine != null) { StopCoroutine(fadeCoroutine); fadeCoroutine = null; }
        fadeCoroutine = StartCoroutine(FadeCanvasGroupRoutine(dialogueCanvasGroup, fadeIn ? 0f : dialogueCanvasGroup.alpha, fadeIn ? 1f : 0f, fadeDuration));
        yield return fadeCoroutine;
        fadeCoroutine = null;
    }

    private IEnumerator FadeCanvasGroupRoutine(CanvasGroup cg, float startAlpha, float endAlpha, float duration)
    {
        currentState = DialogueState.Fading;

        float t = 0f;
        // If startAlpha isn't current, sample current alpha so fades are smooth if interrupted
        startAlpha = cg != null ? cg.alpha : startAlpha;

        while (t < duration)
        {
            if (cg != null) cg.alpha = Mathf.Lerp(startAlpha, endAlpha, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        if (cg != null) cg.alpha = endAlpha;

        // Clear UI on fade-out to avoid flash next time
        if (Mathf.Approximately(endAlpha, 0f))
        {
            ClearTextsImmediate();
            HideCanvasImmediate();
        }

        // If there is a sequence active, move back to Playing; otherwise Idle
        currentState = sequenceQueue.Count > 0 || displayCoroutine != null ? DialogueState.Playing : DialogueState.Idle;
    }

    private void ClearDialogueTextBeforeShow()
    {
        // Clear immediately before fade-in so you don't see old text
        if (dialogueTextUI != null) dialogueTextUI.text = "";
        if (nameTextUI != null) nameTextUI.text = "";
    }

    private void ClearTextsImmediate()
    {
        if (dialogueTextUI != null) dialogueTextUI.text = "";
        if (nameTextUI != null) nameTextUI.text = "";
    }

    private void ShowCanvasImmediate()
    {
        if (dialogueCanvas != null) dialogueCanvas.enabled = true;
        if (dialogueCanvasGroup != null) dialogueCanvasGroup.alpha = 0f;
    }

    private void HideCanvasImmediate()
    {
        if (dialogueCanvas != null) dialogueCanvas.enabled = false;
        if (dialogueCanvasGroup != null) dialogueCanvasGroup.alpha = 0f;
    }

    private void OnDialogueEndSafe(int sequenceIndex)
    {
        try
        {
            OnDialogueEnd?.Invoke(sequenceIndex);
        }
        catch (Exception e)
        {
            Debug.LogWarning("OnDialogueEnd handler threw: " + e);
        }
    }

    #endregion
}