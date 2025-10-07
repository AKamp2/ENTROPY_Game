using UnityEngine;
using System.Collections;

public class ShipAmbience : MonoBehaviour
{
    [Header("Background Noise")]
    public AudioClip backgroundNoise;
    public float crossfadeTime = 5f;
    public float fadeInDuration = 8f;
    public AudioSource sourceA;
    public AudioSource sourceB;

    [Header("Random Creaks")]
    public AudioClip[] creakClips;
    public float minCreakDelay = 10f;
    public float maxCreakDelay = 45f;
    public AudioSource creakSource;

    private bool isSourceAPlaying = true;

    // Call this manually when player spawns
    public void Start()
    {
        if (sourceA != null)
        {
            sourceA.clip = backgroundNoise;
            sourceA.loop = true;
            sourceA.volume = 0f;
        }

        if (sourceB != null)
        {
            sourceB.clip = backgroundNoise;
            sourceB.loop = true;
            sourceB.volume = 0f;
        }

        // Play first background layer with fade-in
        sourceA.Play();
        StartCoroutine(FadeInRoutine(sourceA, fadeInDuration));

        // Begin coroutines
        //StartCoroutine(LoopWithCrossfade());
        StartCoroutine(PlayRandomCreaks());
    }

    private IEnumerator FadeInRoutine(AudioSource source, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, 1f, t / duration);
            yield return null;
        }
        source.volume = 1f;
    }

/*    private IEnumerator LoopWithCrossfade()
    {
        while (true)
        {
            AudioSource current = isSourceAPlaying ? sourceA : sourceB;
            AudioSource next = isSourceAPlaying ? sourceB : sourceA;

            if (current == null || next == null || current.clip == null)
                yield break;

            yield return new WaitForSeconds(current.clip.length - crossfadeTime);

            next.Play();

            float t = 0f;
            while (t < crossfadeTime)
            {
                t += Time.deltaTime;
                float normalized = t / crossfadeTime;

                current.volume = Mathf.Lerp(1f, 0f, normalized);
                next.volume = Mathf.Lerp(0f, 1f, normalized);

                yield return null;
            }

            current.Stop();
            isSourceAPlaying = !isSourceAPlaying;
        }
    }*/

    private IEnumerator PlayRandomCreaks()
    {
        while (true)
        {
            float waitTime = Random.Range(minCreakDelay, maxCreakDelay);
            yield return new WaitForSeconds(waitTime);

            if (creakSource != null && creakClips.Length > 0)
            {
                int index = Random.Range(0, creakClips.Length);
                creakSource.PlayOneShot(creakClips[index]);
            }
        }
    }
}
