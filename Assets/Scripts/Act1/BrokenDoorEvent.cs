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

        //flicker lights
        lightManager.FlickerLights(LightLocation.Dining, 1.0f, 2.0f, true);
    }

    private IEnumerator BrokenDoorBeat()
    {
        yield return new WaitUntil(() => manager.IsDialogueSpeaking == false);
        manager.StartDialogueSequence(3, delay);

        yield return new WaitForSeconds(16f);
        brokenDoorAudio.Play();
        yield return new WaitForSeconds(3f);
        brokenDoor.SetState(DoorScript.States.Broken);
        monitor.CompleteObjective();
    }
}
