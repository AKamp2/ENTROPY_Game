using System.Collections;
using UnityEngine;

public class Sparks : MonoBehaviour
{
    public ParticleSystem spark;
    public AudioSource audio;
    public AudioClip sparkSFX;
    public bool enabled = true;

    public float minBurstDelay = 1f;
    public float maxBurstDelay = 4f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(BurstLoop());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator BurstLoop()
    {
        while (true)
        {
            if (spark.emission.enabled)
            {
                float delay = Random.Range(minBurstDelay, maxBurstDelay);
                yield return new WaitForSeconds(delay);

                spark.Emit(Random.Range(3, 10));
                audio.PlayOneShot(sparkSFX);
            }
            else
            {
                yield return new WaitForSeconds(0.1f); // Prevent CPU spin
            }
        }
    }

    public void ToggleSparks(bool value)
    {
        var emission = spark.emission;
        emission.enabled = value;
        enabled = value;
    }

}
