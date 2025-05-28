using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VentScript : MonoBehaviour
{
	[SerializeField]
	private Rigidbody rb;

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


    // Start is called before the first frame update
    void Start()
    {
        rb = GameObject.Find("Player").GetComponentInChildren<Rigidbody>();

        timer = activeTimeDuration - timeOffset;
    }

    // Update is called once per frame
    void Update()
    {
        if (useTimers)
        {
            timer -= Time.deltaTime;

            // Check for when timer is over
            if (timer <= 0)
            {
                // sets the collider to active/inactive
                bc.enabled = !bc.enabled;

                // handle particle system based on the new enabled state for the collider
                if (bc.enabled)
                {
                    ps.Play();
                    timer = activeTimeDuration;
                }
                else
                {
                    ps.Stop();
                    timer = inactiveTimeDuration;
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
