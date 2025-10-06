using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class EnvironmentAudio : MonoBehaviour
{
    [Header("Doors")]
    public GameObject doorsContainer;
    private DoorScript[] doors;

    [Header("Audio Mixer Groups")]
    public AudioMixerGroup environmentGroup;

    [Header("Hazard Lights")]
    [SerializeField] private HazardLight[] hazardLights;

    private void Start()
    {
        if (doorsContainer != null)
        {
            doors = doorsContainer.GetComponentsInChildren<DoorScript>();
            foreach (DoorScript door in doors)
            {
                if (door.startAudioSource != null)
                    door.startAudioSource.outputAudioMixerGroup = environmentGroup;
                if (door.middleAudioSource != null)
                    door.middleAudioSource.outputAudioMixerGroup = environmentGroup;
                if (door.endAudioSource != null)
                    door.endAudioSource.outputAudioMixerGroup = environmentGroup;

                door.audioManager = this;
            }
        }
    }

    public void FadeInHazardAlarms(float fadeDuration)
    {
        foreach (var hazard in hazardLights)
        {
            if (hazard != null)
            {
                hazard.PlayAlarm();
                StartCoroutine(FadeAudio(hazard, fadeDuration));
            }
        }
    }

    private IEnumerator FadeAudio(HazardLight hazard, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            hazard.SetVolume(Mathf.Lerp(0f, 1f, elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }

        hazard.SetVolume(1f);
    }
}



// Optional: methods to play door sounds if needed
/*
public void PlayDoorOpenAudio(float speed, DoorScript door)
{
    StartCoroutine(DoorOpenCoroutine(speed, door));
}

private IEnumerator DoorOpenCoroutine(float speed, DoorScript door)
{
    AudioSource source = door.audioSource;
    source.PlayOneShot(doorOpenClick);
    source.clip = doorMoving;
    source.pitch = 1f + (speed / 5f) * 0.3f;
    source.Play();

    yield return new WaitUntil(() => door.DoorState == DoorScript.States.Open);

    source.Stop();
    source.PlayOneShot(doorClosingClick);
}
*/

