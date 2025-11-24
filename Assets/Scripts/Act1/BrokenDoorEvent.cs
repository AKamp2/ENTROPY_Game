using UnityEngine;
using System.Collections;

public class BrokenDoorEvent : MonoBehaviour
{
    [SerializeField]
    DoorScript brokenDoor;
    [SerializeField]
    LightManager lightManager;
    [SerializeField]
    float delay = 0f;
    [SerializeField]
    private AudioSource brokenDoorAudio;
    private DialogueManager manager;
    private WristMonitor monitor;
    public StingerManager stingerManager;
    [SerializeField] private Checkpoint diningCheckpoint;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        manager = FindFirstObjectByType<DialogueManager>();
        monitor = FindFirstObjectByType<WristMonitor>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartBrokenDoorBeat()
    {
        StartCoroutine(BrokenDoorBeat());
        stingerManager.BrokenDoorStingerTriggered();
        //flicker lights
        StartCoroutine(lightManager.FlickerLights(LightLocation.Dining, 1.0f, 2.5f, true));
    }

    private IEnumerator BrokenDoorBeat()
    {
        manager.StartDialogueSequence(3, delay);

        yield return new WaitForSeconds(16f);
        brokenDoorAudio.Play();
        yield return new WaitForSeconds(3f);
        brokenDoor.SetState(DoorScript.States.Broken);
        diningCheckpoint.TriggerCheckpointManually();
        monitor.CompleteObjective();
    }
}
