using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VentScript : MonoBehaviour
{
    [SerializeField] private float thrust = 10.0f;
    [SerializeField] private bool useTimers = false;
    [SerializeField] private float activeTimeDuration = 6.0f;
    [SerializeField] private float inactiveTimeDuration = 3.0f;
    [SerializeField] private float timeOffset = 0.0f;
    [SerializeField] private BoxCollider bc;
    [SerializeField] private ParticleSystem ps;
    [SerializeField] private AudioSource ventAudio;
    [SerializeField] private float ventVolume;

    private float timer;
    private bool isActive = true;

    private HashSet<Rigidbody> affectedRigidbodies = new HashSet<Rigidbody>();

    void Start()
    {
        // Start with offset timer
        timer = activeTimeDuration - timeOffset;

        if (ps != null) ps.Play();
        if (ventAudio != null) ventAudio.Play();
        if (bc != null) bc.enabled = true;
        ventVolume = ventAudio.volume;

        
    }

    void Update()
    {
        if (useTimers)
        {
            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                isActive = !isActive;
                bc.enabled = isActive;

                if (isActive)
                {
                    StartCoroutine(Fade(ventAudio, 0, 0.1f, true, ventVolume));
                    ps?.Play();
                    ventAudio?.Play();
                    timer = activeTimeDuration;
                }
                else
                {
                    StartCoroutine(Fade(ventAudio, 0, 0.1f, false, ventVolume));
                    ps?.Stop();
                    ventAudio?.Stop();
                    timer = inactiveTimeDuration;
                    affectedRigidbodies.Clear(); // Stop affecting when disabled
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (!isActive) return;

        foreach (var rb in affectedRigidbodies)
        {
            if (rb != null)
            {
                rb.AddForce(transform.up * thrust);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        if (other.CompareTag("PickupObject") || other.CompareTag("Player"))
        {
            Rigidbody rb = other.attachedRigidbody;
            if (rb != null && !affectedRigidbodies.Contains(rb))
            {
                affectedRigidbodies.Add(rb);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && affectedRigidbodies.Contains(rb))
        {
            affectedRigidbodies.Remove(rb);
        }
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