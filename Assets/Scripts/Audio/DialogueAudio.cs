using UnityEngine;
using UnityEngine.Audio;

public class DialogueAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource dialogueAudioSource;
    public AudioSource sfxAudioSource;

    [Header("SFX Clips")]
    public AudioClip tutorialJingle;

    [Header("Audio Mixer Groups")]
    public AudioMixerGroup dialogueGroup;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dialogueAudioSource.outputAudioMixerGroup = dialogueGroup;
        sfxAudioSource.outputAudioMixerGroup = dialogueGroup;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayJingle()
    {
        sfxAudioSource.PlayOneShot(tutorialJingle);
    }
}
