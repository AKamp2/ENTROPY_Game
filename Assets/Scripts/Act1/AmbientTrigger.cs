using UnityEngine;

public class AmbientTrigger : MonoBehaviour
{

    private AmbientController ambientController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ambientController = FindFirstObjectByType<AmbientController>();
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
            ambientController.Progress();


        }
    }
}
