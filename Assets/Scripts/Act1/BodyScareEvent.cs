using System.Collections;
using UnityEngine;

public class BodyScareEvent : MonoBehaviour
{

    [SerializeField]
    private DoorScript bodyDoor;
    [SerializeField]
    private DoorScript[] doorsToUnlock;
    [SerializeField]
    private GameObject body;
    [SerializeField]
    private Rigidbody bodyRb;
    private DialogueManager dialogueManager;
    

    public GameObject bodyPos;

    public GameplayBeatAudio audioManager;

    private AmbientController ambientController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dialogueManager = FindFirstObjectByType<DialogueManager>();
        ambientController = FindFirstObjectByType<AmbientController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator PlayBodyScare()
    {

        
        

        //bodyRb.AddForce(new Vector3(-.5f, -1, 0) * 30f, ForceMode.Impulse);
        

        bodyDoor.SetState(DoorScript.States.JoltOpen);

        

        //put any body movement Logic here;


        yield return new WaitUntil(() => bodyDoor.aboutToJolt == true);

        body.SetActive(true);
        body.transform.position = bodyPos.transform.position;
        bodyRb.isKinematic = false;

        //bodyRb.AddForce(new Vector3(0f, -1f, 0f) * .5f, ForceMode.Impulse);

        //foreach (Rigidbody rb in bodyRb.GetComponentsInChildren<Rigidbody>())
        //{
        //    rb.AddForce(new Vector3(0f, -1f, 0f) * 5f, ForceMode.Impulse);
        //}

        //yield return new WaitUntil(() => bodyDoor.showingBody);
        audioManager.playBodyStinger();

        foreach(DoorScript door in doorsToUnlock)
        {
            door.SetState(DoorScript.States.Closed);
        }

        yield return new WaitForSeconds(5.5f);

        dialogueManager.StartDialogueSequence(8, 0f);

        yield return new WaitForSeconds(5f);

        ambientController.Progress();


    }
}
