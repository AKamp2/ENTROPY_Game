using UnityEngine;

public class DormHallStingerTrigger : MonoBehaviour
{
    [SerializeField] private AmbientController ambientController;
    [SerializeField] private AudioClip dormHallStingerClip;
    [SerializeField] private float fadeInDuration = 2f;  // you can adjust this
    [SerializeField] private bool disableAfterTrigger = true;

    private bool hasTriggered = false;

    private void Start()
    {
        if (ambientController == null)
            ambientController = FindAnyObjectByType<AmbientController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        hasTriggered = true;

        if (disableAfterTrigger)
        {
            // Optionally disable the collider or the GameObject so it won’t retrigger
            var col = GetComponent<Collider>();
            if (col != null)
                col.enabled = false;
        }

        if (ambientController != null && dormHallStingerClip != null)
        {
            ambientController.PlayStinger(dormHallStingerClip, loop: false, fadeInDuration: fadeInDuration);
        }
    }
}

