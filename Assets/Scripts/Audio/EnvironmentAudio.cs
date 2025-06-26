using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Audio;

public class EnvironmentAudio : MonoBehaviour
{

    public GameObject doorsContainer;
    private DoorScript[] doors;

    [Header("SFX Clips")]
    public AudioClip doorOpenClick;
    public AudioClip doorMoving;
    public AudioClip doorClosingClick;

    [Header("Audio Mixer Groups")]
    public AudioMixerGroup environmentGroup;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        doors = doorsContainer.GetComponentsInChildren<DoorScript>();
        
        foreach(DoorScript door in doors)
        {
            door.audioSource.outputAudioMixerGroup = environmentGroup;
            door.audioManager = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void playDoorOpenAudio(float speed, DoorScript door)
    {
        StartCoroutine(doorOpen(speed, door));
    }

    private IEnumerator doorOpen(float speed, DoorScript door)
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
}
