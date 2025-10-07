using UnityEngine;

public class UIAudioManager : MonoBehaviour
{
    [Header("UI Sounds")]
    public AudioClip hoverClip;
    public AudioClip exitHoverClip;
    public AudioClip selectClip;
    public AudioClip backClip;

    private AudioSource audioSource;

    void Awake()
    {
        // Ensure we have an AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
    }

    public void PlayHoverSound()
    {
        if (hoverClip != null)
            audioSource.PlayOneShot(hoverClip);
    }

    public void PlayExitHoverSound()
    {
        if (exitHoverClip != null)
            audioSource.PlayOneShot(exitHoverClip);
    }

    public void PlaySelectSound()
    {
        if (selectClip != null)
            audioSource.PlayOneShot(selectClip);
    }

    public void PlayBackSound()
    {
        if (backClip != null)
            audioSource.PlayOneShot(backClip);
    }
}


