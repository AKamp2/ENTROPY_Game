using UnityEngine;
using UnityEngine.Audio;

public class PlayerAudio : MonoBehaviour
{
    [Header("Audio Source Prefab")]
    public GameObject audioSourcePrefab; // Must have AudioSource component

    [Header("Audio Parent (for organizing instances)")]
    public Transform audioContainer;

    [Header("Wall Bounce SFX")]
    public AudioClip softBounce;
    public AudioClip hardBounce;
    public AudioClip fatalBounce;

    public AudioMixerGroup playerGroup;

    public void PlaySoftBounce(Vector3 position)
    {
        PlayBounceSoundAtPosition(softBounce, position, 0.3f);
    }

    public void PlayHardBounce(Vector3 position)
    {
        PlayBounceSoundAtPosition(hardBounce, position, 1f);
    }

    public void PlayFatalBounce(Vector3 position)
    {
        PlayBounceSoundAtPosition(fatalBounce, position, 1f);
    }

    private void PlayBounceSoundAtPosition(AudioClip clip, Vector3 position, float volume)
    {
        if (clip == null || audioSourcePrefab == null) return;

        GameObject audioObj = Instantiate(audioSourcePrefab, position, Quaternion.identity, audioContainer);
        AudioSource newSource = audioObj.GetComponent<AudioSource>();
        if (newSource == null) return;

        newSource.clip = clip;
        newSource.outputAudioMixerGroup = playerGroup;
        newSource.volume = volume;
        newSource.pitch = (Random.value / 5f) + 0.85f;
        newSource.Play();

        Destroy(audioObj, clip.length + 0.1f); // Clean up after sound finishes
    }
}
