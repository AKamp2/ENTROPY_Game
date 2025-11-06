using UnityEngine;

public class ExplosionAudioTrigger : MonoBehaviour
{
    public EnvironmentAudio environmentAudio;

    [SerializeField] private bool disableAfterTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (environmentAudio != null)
            {
                environmentAudio.PlayExplosion();
            }
        }
        if (disableAfterTrigger)
        {   
            var col = GetComponent<Collider>();
            if (col != null)
                col.enabled = false;
        }
    }
}
