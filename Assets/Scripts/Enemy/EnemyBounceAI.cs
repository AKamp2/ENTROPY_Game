using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBounceAI : MonoBehaviour
{
    public GameObject trackerObject; // Invisible tracking object
    //public GameObject bouncingObject; // Sphere that bounces
    public float followDistance = 5.0f; // Distance the bouncing object follows behind
    public float bounceSpeed = 5.0f; // Speed of the bouncing object
    public LayerMask barrierLayer; // Layer for walls/barriers

    private Rigidbody bouncingRb;
    public GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        bouncingRb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        MoveBouncingObject();
    }

    private void MoveBouncingObject()
    {
        Vector3 directionToTracker = (trackerObject.transform.position - transform.position).normalized;

        // Apply consistent velocity
        bouncingRb.velocity = directionToTracker * bounceSpeed;

        // Apply collision response for bouncing
        RaycastHit hit;
        if (Physics.Raycast(transform.position, bouncingRb.velocity, out hit, followDistance, barrierLayer))
        {
            // Calculate bounce direction
            Vector3 bounceDirection = Vector3.Reflect(bouncingRb.velocity.normalized, hit.normal);

            // Adjust to ensure the object follows the tracker’s direction partially
            Vector3 weightedDirection = Vector3.Lerp(bounceDirection, directionToTracker, 0.3f).normalized;
            bouncingRb.velocity = weightedDirection * bounceSpeed;
        }
    }

    //void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        // Teleport player to respawn location
    //        player.transform.position = player.GetComponent<ZeroGravity>().respawnLoc.transform.position;

    //        trackerObject.GetComponent<EnemyAI>().Reset();

    //        // Teleport enemy to the starting area
    //        transform.position = trackerObject.transform.position;
    //    }
    //}
}
