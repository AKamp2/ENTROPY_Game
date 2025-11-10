using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class BrokenDoorTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private AmbientController ambientController; // Assign in Inspector
    [SerializeField] private Transform player; // Assign Player transform
    [SerializeField] private Vector3 triggerDirection = Vector3.forward; // The forward direction that counts as "entering"
    [SerializeField] private float fadeInDuration = 3f;
    [SerializeField] private bool disableAfterTrigger = true; // Only allow once

    private bool hasTriggered = false;

    private void Reset()
    {
        // Make sure collider is trigger
        BoxCollider col = GetComponent<BoxCollider>();
        col.isTrigger = true;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        hasTriggered = true;

        // Debug line to confirm trigger
        Debug.Log($"Broken Door Stinger triggered by: {other.name}");

        // Optionally disable collider so it won't retrigger
        var col = GetComponent<Collider>();
        if (col != null && disableAfterTrigger)
            col.enabled = false;


    }

    /*    private void OnTriggerEnter(Collider other)
        {
            if (hasTriggered || ambientController == null) return;
            if (!other.CompareTag("Player")) return;

            // Direction check — only trigger if player enters roughly from the defined direction
            Vector3 toPlayer = (other.transform.position - transform.position).normalized;
            float dot = Vector3.Dot(transform.TransformDirection(triggerDirection).normalized, toPlayer);

            // dot > 0 means the player is moving *towards* the front of the trigger box
            if (dot > 0.3f)
            {
                Debug.Log("BrokenDoor trigger activated by: " + other.name);

                ambientController.PlayStinger(
                    ambientController.BrokenDoorStingerClip,
                    loop: false,
                    fadeInDuration: fadeInDuration
                );

                hasTriggered = true;

                if (disableAfterTrigger)
                    gameObject.SetActive(false);
            }
        }*/
}

