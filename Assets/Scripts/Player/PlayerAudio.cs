using UnityEngine;
using UnityEngine.Audio;

public class PlayerAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource audioSource1;
    public AudioSource audioSource2;
    public AudioSource audioSource3;

    [Header("Wall Bounce SFX")]
    public AudioClip softBounce;
    public AudioClip hardBounce;
    public AudioClip fatalBounce;

    public AudioMixerGroup playerGroup;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource1.outputAudioMixerGroup = playerGroup;
        audioSource2.outputAudioMixerGroup = playerGroup;
        audioSource3.outputAudioMixerGroup = playerGroup;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void playSoftBounce()
    {
        randomizePitch(audioSource1);
        audioSource1.volume = 0.3f;
        audioSource1.PlayOneShot(softBounce);
    }

     public void playHardBounce()
    {
        randomizePitch(audioSource2);
        audioSource2.volume = 1;
        audioSource2.PlayOneShot(hardBounce);
    }

     public void playFatalBounce()
    {
        randomizePitch(audioSource3);
        audioSource3.volume = 1;
        audioSource3.PlayOneShot(fatalBounce);
    }

    void randomizePitch(AudioSource audioSource)
    {
        audioSource.pitch = (Random.value / 5) + 0.85f;
    }
}
