using Mono.Cecil;
using UnityEngine;

public class BarAudioHandler : MonoBehaviour
{
    //[Header("Audio Sources")]
    //public AudioSource audioSource; // The audio source to play the sounds

    [Header("Grab Sounds")]
    public AudioClip[] grabSounds; // Array of possible grab sounds

    [Header("Release Sounds")]
    public AudioClip[] releaseSounds; // Array of possible release sounds


    /// Play a random grab sound.

    public void PlayGrabSound(AudioSource source)
    {
        //Debug.Log(source);
        if (grabSounds.Length == 0 || source == null) return;

        int index = Random.Range(0, grabSounds.Length);
        source.pitch = Random.Range(0.8f, 1.2f);
        source.PlayOneShot(grabSounds[index]);
        source.pitch = 1;
    }
    



    /// Play a random release sound.
  
    public void PlayReleaseSound(AudioSource source)
    {
        //Debug.Log(source);
        if (releaseSounds.Length == 0 || source == null) return;

        int index = Random.Range(0, releaseSounds.Length);
        source.pitch = Random.Range(0.8f, 1.2f);
        source.PlayOneShot(releaseSounds[index]);
    }
}



