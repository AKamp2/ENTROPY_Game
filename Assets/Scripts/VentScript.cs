using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VentScript : MonoBehaviour
{
	[SerializeField]
	private Rigidbody rb;
    [SerializeField]
    private float thrust = 10.0f;

    private bool inRegion = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GameObject.Find("Player").GetComponentInChildren<Rigidbody>();
    }

    // Update is called once per frame
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
