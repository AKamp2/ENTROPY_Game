using UnityEngine;

public class BodyOpenTrigger : MonoBehaviour
{
    [SerializeField]
    private BodyScareEvent bodyScareEvent;
    [SerializeField]
    private Collider player;


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
        if (other == player)
        {
            this.GetComponent<Collider>().enabled = false;
            StartCoroutine(bodyScareEvent.PlayBodyScare());
   
        }
    }
}
