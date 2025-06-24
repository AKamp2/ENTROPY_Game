using UnityEngine;

public class Trigger : MonoBehaviour
{
    [SerializeField]
    private LockdownEvent lde;
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
            lde.OpenDoors();
        }
    }
}
