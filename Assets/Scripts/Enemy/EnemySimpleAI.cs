using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EnemySimpleAI : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 3.0f;
    public float chaseDistance = 10.0f;
    public float escapeDistance = 15f;
    public bool useRandomRoaming;

    [Header("Lunge Settings")]
    public float lungeDistance = 5f;
    public float chargeUpTime = 1.5f;
    public float lungeSpeed = 3.5f;
    public float lungeDuration = 3f;
    public string wallTag = "Barrier";

    [Header("Stun Settings")]
    public float stunSeconds = 3f;
    public float stunVelocityThreshold = 3f;

    [Header("References")]
    public GameObject player;
    public Waypoint startingWaypoint;
    public Transform waypointGroup;
    public DoorScript door;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioSource audioSource2;
    public AudioClip alienSfx;
    public AudioClip chargingSound;
    public AudioClip lungeSound;
    public AudioClip takeDamage;

    // Internal state
    private Waypoint currentWaypoint;
    private Queue<Waypoint> path = new Queue<Waypoint>();

    private float distanceToPlayer;
    private bool isChasingPlayer = false;
    private bool isStunned = false;

    private bool isCharging = false;
    private bool isLunging = false;
    private float lungeTimer = 0f;
    

    void Start()
    {
        // Set the current waypoint to the starting one
        currentWaypoint = startingWaypoint;
        FindPlayerPath();

        audioSource.clip = alienSfx;
        audioSource.loop = true;
        audioSource.Stop();

        // Rigidbody must exist and start in kinematic mode
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        // Only start AI behavior if the door is open
        //if (door.DoorState != DoorScript.States.Open) return;

        // 2) If stunned, do nothing else (charging/lunging will abort)
        if (isStunned) return;

        // 3) If mid-lunge, track timeout
        if (isLunging)
        {
            lungeTimer += Time.deltaTime;
            if (lungeTimer >= lungeDuration)
                EndLunge();
            return;
        }

        // 4) If mid-charge, skip normal AI
        if (isCharging) return;

        // 5) Check if player is within lungeDistance → start charging
        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distanceToPlayer <= lungeDistance)
        {
            StartCoroutine(ChargeAndLunge());
            return;
        }

        // 6) Otherwise, run normal AI (chase/roam/track)
        RunNormalAIBehavior();        

    }

    private void RunNormalAIBehavior()
    {
        

        if (isChasingPlayer)
        {
            if (!audioSource.isPlaying) audioSource.Play();

            if (distanceToPlayer >= escapeDistance)
                isChasingPlayer = false;
            else
                ChasePlayer();
        }
        else
        {
            if (distanceToPlayer <= chaseDistance)
            {
                isChasingPlayer = true;
                ChasePlayer();
            }
            else if (useRandomRoaming)
            {
                isChasingPlayer = false;
                RoamArea();
            }
            else
            {
                isChasingPlayer = false;
                TrackPlayer();

                if (audioSource.isPlaying) audioSource.Stop();
            }
        }
    }

    // Called by Unity when this collider hits another collider
    private void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;

        // 1) If its a thrown pickup object, stun
        if (other.CompareTag("PickupObject"))
        {
            Rigidbody objRb = collision.rigidbody;
            if (objRb != null && objRb.linearVelocity.magnitude >= stunVelocityThreshold)
            {
                StartCoroutine(StunCoroutine());
            }
        }
        // 2) If its the player, kill them
        else if (other.CompareTag("Player"))
        {
            player.GetComponent<ZeroGravity>().IsDead = true;
        }
    }

    private IEnumerator StunCoroutine()
    {
        if (isStunned) yield break;

        // Cancel any ongoing charge or lunge
        isStunned = true;
        if (isCharging)
        {
            isCharging = false;
            // We let the ChargeAndLunge coroutine exit gracefully on its next frame check.
        }
        if (isLunging)
        {
            EndLunge();
        }

        //sfx
        audioSource.Stop();
        audioSource2.Stop();
        audioSource2.PlayOneShot(takeDamage);
        

        // Wait for stun duration
        yield return new WaitForSeconds(stunSeconds);

        // Un-stun and restore AI
        isStunned = false;
        FindPlayerPath();
    }

    void ChasePlayer()
    {
        // Move directly towards the player
        transform.position = Vector3.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
    }

    void TrackPlayer()
    {
        if (path.Count == 0) return;

        Waypoint targetWaypoint = path.Peek();
        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.transform.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetWaypoint.transform.position) < 0.1f)
        {
            path.Dequeue();
        }

        // Recalculate path if path is empty
        if (path.Count == 0) FindPlayerPath();
    }

    void RoamArea()
    {
        // Choose a random neighbor if at the current waypoint
        if (Vector3.Distance(transform.position, currentWaypoint.transform.position) < 0.1f)
        {
            Waypoint nextWaypoint = currentWaypoint.neighbors[Random.Range(0, currentWaypoint.neighbors.Count)];
            currentWaypoint = nextWaypoint;
        }

        transform.position = Vector3.MoveTowards(transform.position, currentWaypoint.transform.position, speed * Time.deltaTime);

        // Switch to chase mode if player is nearby
        if (Vector3.Distance(transform.position, player.transform.position) <= chaseDistance)
        {
            isChasingPlayer = true;
        }
    }

    private IEnumerator ChargeAndLunge()
    {
        isCharging = true;
        isChasingPlayer = false;

        // (Optional: play a charge animation here)
        audioSource.Stop();
        audioSource2.clip = chargingSound;
        audioSource2.Play();

        float timer = 0f;
        while (timer < chargeUpTime)
        {
            if (isStunned)
            {
                isCharging = false;
                yield break;
            }
            timer += Time.deltaTime;
            yield return null;
        }
        audioSource2.Stop();
        audioSource2.clip = lungeSound;
        audioSource2.Play();

        // Finish charging → launch the lunge
        Vector3 dir = (player.transform.position - transform.position).normalized;
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.linearVelocity = dir * lungeSpeed;

        isLunging = true;
        lungeTimer = 0f;
        isCharging = false;
    }

    private void EndLunge()
    {
        if (!isLunging) return;

        isLunging = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        FindPlayerPath();
    }

    void FindPlayerPath()
    {
        Waypoint playerWaypoint = FindClosestWaypoint(player.transform.position);
        path = BFS(currentWaypoint, playerWaypoint);
    }

    Queue<Waypoint> BFS(Waypoint start, Waypoint goal)
    {
        Queue<Waypoint> queue = new Queue<Waypoint>();
        Dictionary<Waypoint, Waypoint> cameFrom = new Dictionary<Waypoint, Waypoint>();
        queue.Enqueue(start);
        cameFrom[start] = null;

        while (queue.Count > 0)
        {
            Waypoint current = queue.Dequeue();
            if (current == goal) break;

            foreach (Waypoint neighbor in current.neighbors)
            {
                if (!cameFrom.ContainsKey(neighbor))
                {
                    queue.Enqueue(neighbor);
                    cameFrom[neighbor] = current;
                }
            }
        }

        Stack<Waypoint> reversePath = new Stack<Waypoint>();
        for (Waypoint at = goal; at != null; at = cameFrom[at])
        {
            reversePath.Push(at);
        }

        Queue<Waypoint> path = new Queue<Waypoint>();
        while (reversePath.Count > 0)
        {
            path.Enqueue(reversePath.Pop());
        }
        return path;
    }

    Waypoint FindClosestWaypoint(Vector3 position)
    {
        Waypoint[] waypoints = waypointGroup.GetComponentsInChildren<Waypoint>();
        Waypoint closest = null;
        float minDist = Mathf.Infinity;

        foreach (Waypoint waypoint in waypoints)
        {
            float dist = Vector3.Distance(position, waypoint.transform.position);
            if (dist < minDist)
            {
                closest = waypoint;
                minDist = dist;
            }
        }
        return closest;
    }
}
