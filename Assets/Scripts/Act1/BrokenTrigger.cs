using UnityEngine;

public class BrokenTrigger : MonoBehaviour
{
    [SerializeField]
    DoorScript brokenDoor;

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
            brokenDoor.SetState(DoorScript.States.Broken);

        }
    }
}
