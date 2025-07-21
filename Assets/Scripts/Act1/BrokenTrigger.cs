using System.Collections;
using UnityEngine;

public class BrokenTrigger : MonoBehaviour
{
    [SerializeField]
    DoorScript brokenDoor;
    [SerializeField]
    GameObject terminalWindow;
    [SerializeField]
    DialogueManager manager;


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
        if (other.CompareTag("Player"))
        {
            this.GetComponent<Collider>().enabled = false;
            terminalWindow.SetActive(true);
            StartCoroutine(BreakDoorWhenFinished());
            

        }
    }

    private IEnumerator BreakDoorWhenFinished()
    {
        //yield return new WaitUntil(() => manager.IsDialogueSpeaking == false);
        yield return new WaitForSeconds(10f);
        brokenDoor.SetState(DoorScript.States.Broken);

    }
}
