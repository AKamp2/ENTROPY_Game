using UnityEngine;

public class AutomaticDoorTrigger : MonoBehaviour
{
    [SerializeField]
    DoorScript ds;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ds = this.GetComponentInParent<DoorScript>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("hitting");

            ds.InRange = true;

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ds.InRange = false;

        }
    }
}
