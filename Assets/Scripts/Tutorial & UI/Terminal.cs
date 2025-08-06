using UnityEngine;
using UnityEngine.InputSystem;

public class Terminal : MonoBehaviour
{
    private ZeroGravity playerScript;
    private GameObject playerObj;
    [SerializeField] GameObject terminalScreen;
    [SerializeField] GameObject targetTransform;
    public bool isLookedAt;
    private void Start()
    {
        playerScript = FindFirstObjectByType<ZeroGravity>();
        playerObj = playerScript.gameObject;
        isLookedAt = false;
    }
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed && isLookedAt)
        { 
            playerScript.PlayerCutSceneHandler(true);
            terminalScreen.SetActive(true);
            playerObj.transform.position = targetTransform.transform.position;
        }
    }
}
