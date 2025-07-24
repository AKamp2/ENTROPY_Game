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
                    ps?.Play();
                    ventAudio?.Play();
                    timer = activeTimeDuration;
                }
                else
                {
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
}