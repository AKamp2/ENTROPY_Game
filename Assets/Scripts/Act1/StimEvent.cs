using System.Collections;
using UnityEngine;


public class StimEvent : MonoBehaviour
{

    private DialogueManager manager;
    [SerializeField]
    private StimDispenser dispenser;
    private ZeroGravity playerScript;

    [SerializeField]
    private DoorScript doorToOpen;

    [SerializeField]
    private CanvasGroup stimUseCanvasGroup;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        manager = FindFirstObjectByType<DialogueManager>();
        playerScript = FindFirstObjectByType<ZeroGravity>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartStimEvent()
    {
        StartCoroutine(StimTutorial());
    }

    public IEnumerator StimTutorial()
    {
        manager.StartDialogueSequence(5, 0.5f);

        yield return new WaitForSeconds(6f);
        dispenser.ToggleUsability(true);

        yield return new WaitUntil(() => playerScript.NumStims == 3);

        StartCoroutine(FadeCanvasGroup(stimUseCanvasGroup, 0f, 1f));

        yield return new WaitUntil(() => playerScript.NumStims < 3);

        StartCoroutine(FadeCanvasGroup(stimUseCanvasGroup, 1f, 0f));

        yield return new WaitForSeconds(2f);

        doorToOpen.SetState(DoorScript.States.Closed);


    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha)
    {
        float timeElapsed = 0f;
        float fadeDuration = 1f;

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
