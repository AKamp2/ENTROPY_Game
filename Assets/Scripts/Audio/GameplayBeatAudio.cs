using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class GameplayBeatAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource bodyStingerSource;
    public AudioSource[] serverSources;
    public AudioSource[] lockdownSources;
    public AudioSource leverSource;
    public AudioSource buttonSource;
    public AudioSource ventSource;
    public AudioSource grateSource;

    [Header("SFX Clips")]
    //public AudioClip bodyFoundStinger;
    public AudioClip powerCutSFX;
    public AudioClip powerOnSFX;
    public AudioClip leverSFX;
    public AudioClip takeItem;
    public AudioClip serverHum;
    public AudioClip buttonPress;
    public AudioClip alienRunAway;
    public AudioClip moveGrate;

    //[Header("Audio Mixer Groups")]
    //public AudioMixerGroup environmentalGroup;
    //public AudioMixerGroup ambienceGroup;

    private float serverVolume;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //foreach(AudioSource source in lockdownSources)
        //{
        //    source.outputAudioMixerGroup = environmentalGroup;
        //}

        //foreach (AudioSource source in serverSources)
        //{
        //    source.outputAudioMixerGroup = environmentalGroup;
        //}
        //buttonSource.outputAudioMixerGroup = environmentalGroup;
        //bodyStingerSource.outputAudioMixerGroup = ambienceGroup;

        if(serverSources[0] != null)
        {
            serverVolume = serverSources[0].volume;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void playMonitorPickup()
    {
        bodyStingerSource.PlayOneShot(takeItem);
    }

/*    public void playBodyStinger()
    {
        bodyStingerSource.PlayOneShot(bodyFoundStinger);
    }*/

    public void playPowerCut()
    {
        foreach(AudioSource source in lockdownSources)
        {
            source.clip = powerCutSFX;
            source.Play();
        }
    }

    public void playPowerOn()
    {
        foreach (AudioSource source in lockdownSources)
        {
            source.clip = powerOnSFX;
            source.Play();
        }
    }

    public void playLeverSFX()
    {
        leverSource.PlayOneShot(leverSFX);
    }


    public void FadeServers(bool fadeIn)
    {
        foreach(AudioSource source in serverSources)
        {
            StartCoroutine(Fade(source, 0, 2f, fadeIn, serverVolume));
        }
    }

    public void PlayButtonClick()
    {
        buttonSource.PlayOneShot(buttonPress);
    }

    public void playAlienRunAway()
    {
        ventSource.clip = alienRunAway;
        ventSource.Play();
    }

    public void PlayMoveGrate()
    {
        grateSource.clip = moveGrate;
        grateSource.Play();
    }

    public IEnumerator Fade(AudioSource source, float timeBeforeFade, float fadeDuration, bool fadeIn, float originalVolume)
    {
        yield return new WaitForSecondsRealtime(timeBeforeFade);

        float startVolume = fadeIn ? 0f : originalVolume;
        float endVolume = fadeIn ? originalVolume : 0f;

        double startTime = AudioSettings.dspTime;
        double endTime = startTime + fadeDuration;

        source.volume = startVolume;

        if (fadeIn && !source.isPlaying)
            source.Play();

        while (AudioSettings.dspTime < endTime)
        {
            float t = (float)((AudioSettings.dspTime - startTime) / fadeDuration);
            source.volume = Mathf.Lerp(startVolume, endVolume, t);
            yield return null;
        }

        source.volume = endVolume;

        if (!fadeIn)
            source.Stop();
    }
}
