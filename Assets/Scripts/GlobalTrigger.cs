using UnityEngine;
using UnityEngine.Events;

public class GlobalTrigger : MonoBehaviour
{

    [Header("Events")]
    public UnityEvent triggeredEvent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            this.GetComponent<Collider>().enabled = false;
            triggeredEvent?.Invoke();

        }
    }
}
