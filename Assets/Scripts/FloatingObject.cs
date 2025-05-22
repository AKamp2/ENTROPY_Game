using UnityEngine;

public class FloatingObject : MonoBehaviour
{

    Rigidbody rb;

   

    [SerializeField]
    bool useDirectionInput = false;
    [SerializeField]
    Vector3 initDirection;

    [SerializeField]
    bool useSpeedInput = false;
    [SerializeField]
    float initSpeed = 100f;

    [SerializeField]
    float randomSpeedLower = 25f;
    [SerializeField]
    float randomSpeedUpper = 100f;


 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        rb = this.gameObject.GetComponent<Rigidbody>();


        if (!useSpeedInput)
        {
            initSpeed = Random.Range(randomSpeedLower, randomSpeedUpper);
        }
        
        // very basic scaling: more mass = less speed
        initSpeed = initSpeed / rb.mass;

        // if no manual input direction, randomly create direction
        if (!useDirectionInput)
        {
            initDirection = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
                );

            initDirection.Normalize();

        }

        // add initial forces
        rb.AddTorque( initDirection * (initSpeed * 0.1f));
        rb.AddForce(initDirection * initSpeed);


    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (rb.linearVelocity.magnitude <= 0.3)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * 0.3f;

        }
                

            
        Debug.Log(rb.linearVelocity.magnitude);
    }
}
