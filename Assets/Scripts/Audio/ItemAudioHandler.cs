using UnityEngine;

public class ItemAudioHandler : MonoBehaviour
{
    [Header("Grab Sounds")]
    public AudioClip[] pickupSounds;

    [Header("Throw Sounds")]
    public AudioClip[] throwSounds;

    public GameObject audioSourcePrefab;
    public Transform audioSourceContainer;

    public void PlayPickUpSound(Vector3 position)
    {
        if (pickupSounds.Length == 0) return;

        int index = Random.Range(0, pickupSounds.Length);
        PlaySoundAtPosition(pickupSounds[index], position);
    }

    public void PlayThrowSound(Vector3 position)
    {
        if (throwSounds.Length == 0) return;

        int index = Random.Range(0, throwSounds.Length);
        PlaySoundAtPosition(throwSounds[index], position);
    }

    private void PlaySoundAtPosition(AudioClip clip, Vector3 position)
    {
        if (clip == null || audioSourcePrefab == null) return;

        GameObject tempAudioObj =
            Instantiate(audioSourcePrefab, position, Quaternion.identity, audioSourceContainer);

        AudioSource tempSource = tempAudioObj.GetComponent<AudioSource>();

        if (tempSource == null)
        {
            Debug.LogError("Audio prefab missing AudioSource!");
            Destroy(tempAudioObj);
            return;
        }

        tempSource.clip = clip;
        tempSource.pitch = Random.Range(0.8f, 1.2f);
        tempSource.spatialBlend = 1f; // Ensure 3D sound
        tempSource.Play();

        Destroy(tempAudioObj, clip.length + 0.1f);
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

