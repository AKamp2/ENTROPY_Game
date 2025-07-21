using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField]
    DialogueManager manager;
    [SerializeField]
    float delay = 0f;
    [SerializeField]
    private Collider player;
    [SerializeField]
    WristMonitor monitor;

    [SerializeField]
    int index;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other == player)
        {
            Debug.Log("player collided");
            //ensure it only happens
            this.GetComponent<Collider>().enabled = false;
            //sart the dialogue
            manager.StartDialogueSequence(index, delay);
            //update wrist monitor objective
            monitor.CompleteObjective();
        }
    }
}
