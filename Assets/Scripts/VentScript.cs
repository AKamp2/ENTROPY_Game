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
        
    }

    // Update is called once per frame
    void Update()
    {

        if (inRegion)
        {
            rb.AddForce(transform.up * thrust);
        }
        
    }


    void OnTriggerEnter(Collider other)
    {
        inRegion = true;
    }


    void OnTriggerExit(Collider other)
    {
        inRegion = false;
    }
}
