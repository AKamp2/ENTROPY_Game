using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

// A utility class to be used by the ambient controller (and anything else that finds it useful).
// The looper has two audio sources it switches between to make seamless transitions between loops.
// Timing and source control must be done externally, but the looper will manage flipping between players.
public class Looper : MonoBehaviour
{

    private int sourceIndex = 0;
    private AudioSource[] audioSources = new AudioSource[2];
    // private double endTime = 0.0f;
    private float normalVolume = 0.3f;

    public const double fadeDuration = 0.5f;
    public AudioMixerGroup musicGroup;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        for (int i = 0; i < 2; i++)
        {
            GameObject child = new GameObject("LoopPlayer");
            child.transform.parent = gameObject.transform;
            audioSources[i] = child.AddComponent<AudioSource>();
            audioSources[i].loop = false;
        }

        foreach (AudioSource audioSource in audioSources)
        {
            audioSource.outputAudioMixerGroup = musicGroup;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    

    // public void Enqueue(AudioClip clip, double time, double end)
    public void Enqueue(AudioClip clip, double time)
    {
        sourceIndex = 1 - sourceIndex;
        audioSources[sourceIndex].clip = clip;
        audioSources[sourceIndex].volume = normalVolume;
        audioSources[sourceIndex].PlayScheduled(time);
        // endTime = end;
    }

    public void StopAt(double stopTime)
    {
        // Debug.Log("Had to fade");
        float delay = (float)(stopTime - (AudioSettings.dspTime + fadeDuration));
        StartCoroutine(Fade(audioSources[sourceIndex], delay));
    }

    IEnumerator Fade(AudioSource source, float timeBeforeFade)
    {
        
        yield return new WaitForSecondsRealtime(timeBeforeFade);

        double end = AudioSettings.dspTime+fadeDuration;

        while (source.volume > 0)
        {
            source.volume = Mathf.Lerp(1, 0, (float)(AudioSettings.dspTime / end));
            yield return null;
        }
        source.Stop();
    }

    public void SetVolume(float newVolume)
    {
        audioSources[0].volume = newVolume;
        audioSources[1].volume = newVolume;
        normalVolume = newVolume;
    }
}
