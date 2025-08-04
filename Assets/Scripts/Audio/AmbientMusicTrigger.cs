using UnityEngine;

public class AmbientMusicTrigger : MonoBehaviour
{
    [SerializeField]
    private AmbientController controller;
    [SerializeField]
    private bool fadeIn = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (controller == null)
        {
            controller = FindAnyObjectByType<AmbientController>();
        }
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
            if(controller != null)
            {
                controller.FadeAll(fadeIn);
            }
            

        }
    }
}
