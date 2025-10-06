using UnityEngine;

public class HazardLight : MonoBehaviour
{
    [SerializeField] private bool isHazard;
    [SerializeField] private Light light;
    [SerializeField] private Light lightBase;
    [SerializeField] private float rotateParam;
    [SerializeField] private AudioSource hazardAudioSource;

    // Expose AudioSource safely
    public AudioSource HazardAudioSource => hazardAudioSource;

    public bool IsHazard
    {
        get { return isHazard; }
        set { isHazard = value; }
    }

    void Update()
    {
        if (isHazard)
        {
            if (!light.enabled)
            {
                light.enabled = true;
                lightBase.enabled = true;
            }
            light.transform.Rotate(transform.up * rotateParam * Time.deltaTime);

            if (hazardAudioSource != null && !hazardAudioSource.isPlaying)
            {
                hazardAudioSource.Play();
            }
        }
        else
        {
            if (light.enabled)
            {
                light.enabled = false;
                lightBase.enabled = false;
            }

            if (hazardAudioSource != null && hazardAudioSource.isPlaying)
            {
                hazardAudioSource.Stop();
            }
        }
    }

    // Called by EnvironmentAudio to fade in after tutorial
    public void PlayAlarm()
    {
        if (hazardAudioSource != null && !hazardAudioSource.isPlaying)
        {
            hazardAudioSource.volume = 0f;
            hazardAudioSource.loop = true;
            hazardAudioSource.Play();
        }
    }

    public void SetVolume(float volume)
    {
        if (hazardAudioSource != null)
            hazardAudioSource.volume = volume;
    }
}









