using UnityEngine;
using UnityEngine.InputSystem;

public class TerminalManager : MonoBehaviour
{

    private Terminal currentTerminal;
    public void GetCurrentTerminal(Terminal terminal)
    {
        currentTerminal = terminal;
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (currentTerminal != null)
        {
            if (context.performed && currentTerminal.isLookedAt && !currentTerminal.isActivated)
            {
                currentTerminal.Activation();
                
            }
        }
    }
    //What you can do is move the onInteract into this script so that you don't have to give the player input an OnInteract for every single terminal in the game.
    //Then have a reference for currentTerminal in here and update what the currently viewed terminal is by passing a reference of the terminal you're looking at to this script in the UIManager.
    //OnInteract should activate the terminal sequence of the currentTerminal (and it should only do this once, terminals need to be marked as used and you should be checking to see if the terminal is used before displaying the UI)
    //Things like sound effects will be called from the terminal script so that it can easily be localized to that terminal's audio source.
    //if you want the interaction to be a hold like on stim dispenser you can look at the stim dispenser script to see how I did it, possibly use the same UI element as I did for the progress bar.

}
