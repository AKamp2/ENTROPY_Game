using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VentScript : MonoBehaviour
{
	[SerializeField]
	private Rigidbody rb;
    [SerializeField]
    private ZeroGravity zg;

    [SerializeField]
    private BoxCollider bc;

    [SerializeField]
    private ParticleSystem ps;


    [SerializeField]
    private float thrust = 10.0f;

    private bool inRegion = false;

    [SerializeField]
    private bool useTimers = false;
    [SerializeField]
    private float activeTimeDuration = 6.0f;
    [SerializeField]
    private float inactiveTimeDuration = 3.0f;
    [SerializeField]
    private float timeOffset = 0.0f;
    [SerializeField]
    private float timer;

    private bool isActive = true; // Start with active state

    public AudioSource ventAudio;


    // Start is called before the first frame update
    void Start()
    {
        rb = GameObject.Find("Player").GetComponentInChildren<Rigidbody>();
        zg = rb.GetComponent<ZeroGravity>();

        // Initial delay before first switch
        timer = activeTimeDuration - timeOffset;

        ps.Play();
        ventAudio.Play();
        bc.enabled = true;
    }

    void Update()
    {
        if (inRegion && zg.IsDead)
        {
            inRegion = false;
        }

        if (useTimers)
        {
            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                isActive = !isActive;

                bc.enabled = isActive;

                if (isActive)
                {
                    ps.Play();
                    ventAudio.Play();
                    timer = activeTimeDuration;
                }
                else
                {
                    ps.Stop();
                    ventAudio.Stop();
                    timer = inactiveTimeDuration;
                    inRegion = false;
                }
            }
        }
    }

        // FixedUpdate used mainly for physics
        void FixedUpdate()
    {

        if (inRegion)
        {
            rb.AddForce(transform.up * thrust);
        }
        
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (rb != null)
			    inRegion = true;
		}
       
    }


    void OnTriggerExit(Collider other)
    {
		if (other.CompareTag("Player"))
		{
			if (rb != null)
				inRegion = false;
		}
	}
}
