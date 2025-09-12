using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.Events;

public class Terminal : MonoBehaviour
{
    private ZeroGravity playerScript;
    private GameObject playerObj;
    [SerializeField] TerminalScreen terminalScreenScript;
    [SerializeField] GameObject terminalScreenObj;
    [SerializeField] GameObject ALANScreen;
    [SerializeField] GameObject targetTransform;
    public bool isLookedAt = false;
    public bool isActivated = false;
    [SerializeField] TerminalPopup popup;
    [SerializeField] TerminalDisabled disabled;
    private Coroutine disabledRoutine;
    public bool isUploadComplete = false;

    //this is we can assign different actions to be called by our terminals
    //assign a method from a script in the inspector
    [Header("Events")]
    public UnityEvent onUploadComplete;

    private void Start()
    {
        playerScript = FindFirstObjectByType<ZeroGravity>();
        playerObj = playerScript.gameObject;
        if(isActivated == false)
        {
            disabled.StartFlashing();
        }
    }
    
    private void Update()
    {
        if (popup.isUploaded && !isUploadComplete)
        {
            isUploadComplete = true;
            playerScript.PlayerCutSceneHandler(false);
            //terminalScreenObj.SetActive(false);
            ALANScreen.SetActive(true);

            onUploadComplete?.Invoke();
        }
    }

    public void Activation()
    {
        isActivated = true;
        disabled.StopFlashing();
        playerScript.PlayerCutSceneHandler(true);
        terminalScreenObj.SetActive(true);
        terminalScreenScript.StartCoroutine(terminalScreenScript.TypeText());
        StartCoroutine(LerpPosition(targetTransform.transform.position, 0.75f));
    }

    public IEnumerator LerpPosition(Vector3 destination, float duration)
    {
        Vector3 start = playerObj.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            playerObj.transform.position = Vector3.Lerp(start, destination, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        playerObj.transform.position = destination; // Snap to final position
    }
}
