using UnityEngine;
using System.Collections;

public class TerminalAudioManager : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip bootupClip;
    public AudioClip uploadCompleteClip;

    [Header("Audio Sources")]
    public AudioSource bootupSource;            // For bootup sound
    public AudioSource uploadCompleteSource;    // For upload complete sound

    private Coroutine fadeCoroutine;

    /// <summary>
    /// Play the bootup sound with optional fade-in
    /// </summary>
    public void PlayBootupSound(float fadeInDuration = 3f)
    {
        if (bootupSource == null || bootupClip == null) return;

        bootupSource.clip = bootupClip;
        bootupSource.loop = false;
        bootupSource.volume = 0f;
        bootupSource.Play();

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeAudio(bootupSource, 0f, 1f, fadeInDuration));
    }

    /// <summary>
    /// Stop or fade out the bootup sound
    /// </summary>
    public void FadeOutBootup(float fadeOutDuration = 0.5f)
    {
        if (bootupSource == null) return;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeAudio(bootupSource, bootupSource.volume, 0f, fadeOutDuration, stopAfterFade: true));
    }

    /// <summary>
    /// Play the upload complete sound
    /// </summary>
    public void PlayUploadCompleteSound()
    {
        if (uploadCompleteSource == null || uploadCompleteClip == null) return;

        uploadCompleteSource.clip = uploadCompleteClip;
        uploadCompleteSource.loop = false;
        uploadCompleteSource.volume = 1f;
        uploadCompleteSource.Play();
    }

    /// <summary>
    /// Generic coroutine to fade audio
    /// </summary>
    private IEnumerator FadeAudio(AudioSource source, float startVolume, float endVolume, float duration, bool stopAfterFade = false)
    {
        float time = 0f;
        while (time < duration)
        {
            source.volume = Mathf.Lerp(startVolume, endVolume, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        source.volume = endVolume;

        if (stopAfterFade)
            source.Stop();
    }
}





