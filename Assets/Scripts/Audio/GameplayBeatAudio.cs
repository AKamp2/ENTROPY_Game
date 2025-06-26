using UnityEngine;
using UnityEngine.Audio;

public class GameplayBeatAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource bodyStingerSource;
    public AudioSource powerCutSource;

    [Header("SFX Clips")]
    public AudioClip bodyFoundStinger;
    public AudioClip powerCutSFX;
    public AudioClip powerOnSFX;

    [Header("Audio Mixer Groups")]
    public AudioMixerGroup environmentalGroup;
    public AudioMixerGroup ambienceGroup;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        powerCutSource.outputAudioMixerGroup = environmentalGroup;
        bodyStingerSource.outputAudioMixerGroup = ambienceGroup;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void playBodyStinger()
    {
        bodyStingerSource.PlayOneShot(bodyFoundStinger);
    }

    public void playPowerCut()
    {
        powerCutSource.PlayOneShot(powerCutSFX);
    }

    public void playPowerOn()
    {
        powerCutSource.PlayOneShot(powerOnSFX);
    }
}
