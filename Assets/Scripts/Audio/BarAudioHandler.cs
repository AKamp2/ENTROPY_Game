using UnityEngine;

public class BarAudioHandler : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource audioSource; // The audio source to play the sounds

    [Header("Grab Sounds")]
    public AudioClip[] grabSounds; // Array of possible grab sounds

    [Header("Release Sounds")]
    public AudioClip[] releaseSounds; // Array of possible release sounds

    /// <summary>
    /// Play a random grab sound.
    /// </summary>
    public void PlayGrabSound()
    {
        if (grabSounds.Length == 0 || audioSource == null) return;

        int index = Random.Range(0, grabSounds.Length);
        audioSource.PlayOneShot(grabSounds[index]);
    }

    /// <summary>
    /// Play a random release sound.
    /// </summary>
    public void PlayReleaseSound()
    {
        if (releaseSounds.Length == 0 || audioSource == null) return;

        int index = Random.Range(0, releaseSounds.Length);
        audioSource.PlayOneShot(releaseSounds[index]);
    }
}



