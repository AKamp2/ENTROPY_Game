using UnityEngine;
using UnityEngine.InputSystem;

public class Terminal : MonoBehaviour
{
    ZeroGravity playerScript;
    GameObject playerObj;
    [SerializeField] Vector3 targetTransform;
    private void Start()
    {
        playerScript = FindFirstObjectByType<ZeroGravity>();
        playerObj = playerScript.gameObject;
    }
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Turn on terminal 
            // Lerp player to a set place to watch the animation
            playerScript.PlayerCutSceneHandler(true);
            playerObj.transform.position = targetTransform;
        }
    }
}
