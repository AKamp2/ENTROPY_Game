using UnityEngine;

public class AmbientMusicTrigger : MonoBehaviour
{
    [Header("Controller Reference (Optional)")]
    [SerializeField] private AmbientController controller;

    [Header("Stinger Settings")]
    [Tooltip("The AudioSource that will play this stinger.")]
    [SerializeField] private AudioSource stingerSource;

    [Tooltip("The AudioClip this trigger will play.")]
    [SerializeField] private AudioClip stingerClip;

    [Tooltip("Should this stinger loop while the player is in the trigger?")]
    [SerializeField] private bool loopClip = false;

    [Tooltip("Disable the trigger after it's been activated once.")]
    [SerializeField] private bool disableAfterTrigger = true;

    private void Start()
    {
        if (controller == null)
            controller = FindAnyObjectByType<AmbientController>();

        if (stingerSource != null)
            stingerSource.loop = loopClip;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (disableAfterTrigger)
            GetComponent<Collider>().enabled = false;

        if (stingerSource == null || stingerClip == null)
        {
            Debug.LogWarning($"[{name}] Missing stingerSource or stingerClip reference!");
            return;
        }

        // Play the stinger
        stingerSource.clip = stingerClip;
        stingerSource.loop = loopClip;
        stingerSource.Stop(); // ensure a clean start
        stingerSource.Play();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && loopClip && stingerSource != null)
        {
            stingerSource.Stop();
        }
    }
    /*    [SerializeField]
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
        }*/
}
