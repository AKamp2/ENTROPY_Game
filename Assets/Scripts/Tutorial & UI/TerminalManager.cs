using UnityEngine;
using UnityEngine.InputSystem;

public class TerminalManager : MonoBehaviour
{
    [SerializeField] ZeroGravity player;
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed) 
        { 
            // Turn on terminal 
            // Lerp player to a set place to watch the animation
            
        }
    }
}
