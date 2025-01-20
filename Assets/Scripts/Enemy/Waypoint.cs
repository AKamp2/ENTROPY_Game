
// Waypoint.cs
using UnityEngine;
using System.Collections.Generic;

public class Waypoint : MonoBehaviour
{
    public List<Waypoint> neighbors = new List<Waypoint>(); // Adjacent waypoints
    public DoorScript connectedDoor;
    public Color highlightColor = Color.red; // Highlight color for selected waypoint's neighbors
    public Color defaultLineColor = Color.green; // Line color for all neighbors

    private void OnDrawGizmos()
    {
        // Draw a sphere at the waypoint's position for easy identification
        Gizmos.color = Color.yellow;
        //Gizmos.DrawSphere(transform.position, 0.2f);

        // Draw default green lines to all neighbor waypoints
        Gizmos.color = defaultLineColor;
        foreach (Waypoint neighbor in neighbors)
        {
            if (neighbor != null)
            {
                Gizmos.DrawLine(transform.position, neighbor.transform.position);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Highlight the waypoint itself when selected
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 0.25f); // Slightly larger sphere to indicate selection

        // Highlight connections to neighbors with red lines
        Gizmos.color = highlightColor;
        foreach (Waypoint neighbor in neighbors)
        {
            if (neighbor != null)
            {
                Gizmos.DrawLine(transform.position, neighbor.transform.position);

                // Optionally, draw a small sphere at the neighbor's position for clarity
                Gizmos.DrawSphere(neighbor.transform.position, 0.2f);
            }
        }
    }

    public bool DoorIsOpen()
    {
        if (connectedDoor != null)
        {
            if (connectedDoor.DoorState == DoorScript.States.Closed)
            {
                return false;
            }
        }
        return true;

    }
}