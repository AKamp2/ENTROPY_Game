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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dialogueManager = FindFirstObjectByType<DialogueManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator PlayBodyScare()
    {

        body.transform.position = bodyPos.transform.position;
        body.transform.rotation = bodyPos.transform.rotation;

        bodyRb.AddForce(new Vector3(-.5f, -1, 0) * 30f, ForceMode.Impulse);

        bodyDoor.SetState(DoorScript.States.JoltOpen);

        //put any body movement Logic here;


        yield return new WaitUntil(() => bodyDoor.showingBody);

        audioManager.playBodyStinger();

        foreach(DoorScript door in doorsToUnlock)
        {
            door.SetState(DoorScript.States.Closed);
        }

        yield return new WaitForSeconds(4f);

        dialogueManager.StartDialogueSequence(8, 0f);
    }
}
