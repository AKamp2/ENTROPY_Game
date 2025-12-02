using System.Collections;
using UnityEngine;

public class EndLevel1 : MonoBehaviour
{
    private ZeroGravity player;
    private DialogueManager dialogueManager;
    //public GameplayBeatAudio audioManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dialogueManager = FindFirstObjectByType<DialogueManager>();
        player = FindFirstObjectByType<ZeroGravity>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartEndLevel1Event()
    {
        StartCoroutine(Level1End());
    }

    public IEnumerator Level1End()
    {
        dialogueManager.StartDialogueSequence(6, 0f);
        yield return new WaitForSeconds(6f);
    }

    private void OnTriggerEnter(Collider other)
    {
        StartEndLevel1Event();
    }
}
