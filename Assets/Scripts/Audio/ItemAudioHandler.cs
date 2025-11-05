using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ItemAudioHandler : MonoBehaviour
{
    //[Header("Audio Source")]
    //public AudioSource audioSource;

    [Header("Grab Sounds")]
    public AudioClip[] pickupSounds;

    [Header("Throw Sounds")]
    public AudioClip[] throwSounds;

/*    [Header("Impact Sounds")]
    public AudioClip[] impactSounds;

    [Tooltip("Minimum collision velocity to trigger an impact sound")]
    public float impactThreshold = 1.5f;*/

/*    private void Awake()
    {
        
    }*/

    public void PlayPickUpSound(AudioSource source)
    {
        if (pickupSounds.Length == 0 || source == null) return;
        int index = Random.Range(0, pickupSounds.Length);
        source.pitch = Random.Range(0.95f, 1.05f);
        source.PlayOneShot(pickupSounds[index]);
    }

    public void PlayThrowSound(AudioSource source)
    {
        if (throwSounds.Length == 0 || source == null) return;
        int index = Random.Range(0, throwSounds.Length);
        source.pitch = Random.Range(0.95f, 1.05f);
        source.PlayOneShot(throwSounds[index]);
    }
}

/*private void OnCollisionEnter(Collision collision)
{
    // Prevent sounds when object is being held
    if (GetComponent<Rigidbody>()?.isKinematic == true)
        return;

    // Only play if above threshold
    if (collision.relativeVelocity.magnitude >= impactThreshold)
    {
        PlayImpactSound();
    }
}

public void PlayImpactSound()
{
    if (impactSounds.Length == 0 || audioSource == null) return;
    int index = Random.Range(0, impactSounds.Length);
    audioSource.pitch = Random.Range(0.9f, 1.1f);
    audioSource.PlayOneShot(impactSounds[index]);
}*/

