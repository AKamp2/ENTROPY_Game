using UnityEngine;

public class StimTrigger : MonoBehaviour
{
    [SerializeField]
    private GameObject terminalWindow;
    [SerializeField]
    private StimEvent stimEvent;
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
            StartCoroutine(stimEvent.StartStimTutorial());


        }
    }
}
