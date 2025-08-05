using System.Collections;
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
    public bool updateWristMonitor = true;

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

            StartCoroutine(StartDialogue());
        }
    }

    private IEnumerator StartDialogue()
    {
        //start dialogue when it's not talking
        yield return new WaitUntil(() => manager.IsDialogueSpeaking == false);
        manager.StartDialogueSequence(index, delay);
        //update wrist monitor objective
        if(updateWristMonitor)
        {
            monitor.CompleteObjective();
        }
        
    }
}
