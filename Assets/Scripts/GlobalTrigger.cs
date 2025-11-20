using UnityEngine;
using UnityEngine.Events;

public class GlobalTrigger : MonoBehaviour
{

    [Header("Events")]
    [Tooltip("The method that this trigger calls when it's triggered.")]
    public UnityEvent triggeredEvent;

    [Tooltip("Specify a specialized method that is called when useLoadEvent is true, only when the scene is reloaded and the trigger is recalled")]
    public UnityEvent loadEvent;

    [Header("Save Data")]
    
    public bool isTriggered = false;

    [Tooltip("When we reload the scene and this trigger has already been set off, do we call the behavior of this trigger again?")]
    public bool recallUponSceneLoad;

    [Tooltip("When when we recall this trigger, do we use the triggered event or the specialized load event?")]
    public bool useLoadEvent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    /// <summary>
    /// Simple method detects if the player enters the trigger and then disables it and invokes the method that the trigger is supposed to do.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isTriggered = true;
            this.GetComponent<Collider>().enabled = false;
            triggeredEvent?.Invoke();

        }
    }



    public void ResetTrigger()
    {
        this.GetComponent<Collider>().enabled = true;
        isTriggered = false;
    }

    /// <summary>
    /// If we want the trigger to recall its behavior it will do this
    /// </summary>
    public void RecallTriggeredBehavior()
    {
        if (useLoadEvent)
        {
            if (loadEvent != null)
            {
                loadEvent.Invoke();
            }
            else
            {
                Debug.LogError($"No load event for {this.name} has been specified.");
            }
        }
        else
        {
            triggeredEvent?.Invoke();
        }
    }
}
