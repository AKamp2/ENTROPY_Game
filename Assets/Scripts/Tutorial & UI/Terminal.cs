using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Terminal : MonoBehaviour
{
    private ZeroGravity playerScript;
    private GameObject playerObj;
    [SerializeField] TerminalScreen terminalScreenScript;
    [SerializeField] GameObject terminalScreenObj;
    [SerializeField] GameObject targetTransform;
    public bool isLookedAt = false;
    public bool isActivated = false;
    public bool isUploadHidden = false;
    [SerializeField] TerminalPopup popup;
    private void Start()
    {
        playerScript = FindFirstObjectByType<ZeroGravity>();
        playerObj = playerScript.gameObject;
    }
    
    private void Update()
    {
        if (popup.isUploaded && !isUploadHidden)
        {
            isUploadHidden = true;
            playerScript.PlayerCutSceneHandler(false);
            terminalScreenObj.SetActive(false);
        }
    }

    public void Activation()
    {
        isActivated = true;
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
