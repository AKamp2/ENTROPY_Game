using UnityEngine;
using System.Collections.Generic;

public class EnemySimpleAI : MonoBehaviour
{
    public float speed = 2.0f;
    public float chaseDistance = 5.0f;
    public float escapeDistance = 10f;
    public GameObject player;
    public Waypoint startingWaypoint; // Starting waypoint for teleportation
    public DoorScript door;
    public bool useRandomRoaming; // Toggle to switch between roaming and tracking modes

    private Waypoint currentWaypoint;
    private Queue<Waypoint> path = new Queue<Waypoint>();
    private bool isChasingPlayer = false;
    private bool isMoving = true;

    void Start()
    {
        // Set the current waypoint to the starting one
        currentWaypoint = startingWaypoint;
        FindPlayerPath();
    }

    void Update()
    {
        // Only start AI behavior if the door is open
        if (door.DoorState != DoorScript.States.Open) return;


        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (isMoving)
        {
            if (isChasingPlayer)
            {
                if (distanceToPlayer >= escapeDistance)
                {
                    isChasingPlayer = false;
                }
                else
                {
                    ChasePlayer();
                }

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
                }
            }
        }
        

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

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Teleport player to respawn location
            player.transform.position = player.GetComponent<ZeroGravity>().respawnLoc.transform.position;

            // Teleport enemy to the starting waypoint
            transform.position = startingWaypoint.transform.position;

            // Reset path and current waypoint
            currentWaypoint = startingWaypoint;
            FindPlayerPath();
        }
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
        Waypoint[] waypoints = FindObjectsOfType<Waypoint>();
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
