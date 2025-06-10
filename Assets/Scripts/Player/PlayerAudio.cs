using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource audioSource;

    [Header("Wall Bounce SFX")]
    public AudioClip softBounce;
    public AudioClip hardBounce;
    public AudioClip fatalBounce;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void playSoftBounce()
    {
        randomizePitch();
        audioSource.volume = 0.3f;
        audioSource.PlayOneShot(softBounce);
    }

     public void playHardBounce()
    {
        randomizePitch();
        audioSource.volume = 1;
        audioSource.PlayOneShot(hardBounce);
    }

     public void playFatalBounce()
    {
        randomizePitch();
        audioSource.volume = 1;
        audioSource.PlayOneShot(fatalBounce);
    }

    void randomizePitch()
    {
        audioSource.pitch = (Random.value / 2) + 0.75f;
    }
}
