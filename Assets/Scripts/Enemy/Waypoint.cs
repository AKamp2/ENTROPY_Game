
// Waypoint.cs
using UnityEngine;
using System.Collections.Generic;

public class Waypoint : MonoBehaviour
{

    public enum WaypointType
    {
        General,
        Roaming
    }

    [Header("Waypoint Settings")]
    public WaypointType type = WaypointType.General;

    [Header("Connections")]
    public List<Waypoint> neighbors = new List<Waypoint>();
    public DoorScript connectedDoor;

    [Header("Gizmo Settings")]
    public Color highlightColor = Color.red;
    public Color defaultLineColor = Color.green;


    private void OnDrawGizmos()
    {
        // Set sphere color based on waypoint type
        if (type == WaypointType.Roaming)
            Gizmos.color = Color.blue;
        else
            Gizmos.color = Color.yellow;

        // Draw the waypoint sphere
        Gizmos.DrawWireSphere(transform.position, 0.15f); // Adjust size as desired

        // Draw default connection lines to neighbors
        Gizmos.color = defaultLineColor;
        // Draw lines to neighbors with color based on waypoint types
        foreach (Waypoint neighbor in neighbors)
        {
            if (neighbor == null) continue;

            // Determine line color based on types
            if (this.type == WaypointType.Roaming && neighbor.type == WaypointType.Roaming)
            {
                Gizmos.color = Color.blue;  // Both roaming
            }
            else if ((this.type == WaypointType.Roaming && neighbor.type == WaypointType.General) ||
                     (this.type == WaypointType.General && neighbor.type == WaypointType.Roaming))
            {
                Gizmos.color = new Color(1f, 0.65f, 0f);  // Orange color
            }
            else
            {
                Gizmos.color = Color.yellow;  // fallback color
            }

            Gizmos.DrawLine(transform.position, neighbor.transform.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Highlight the waypoint itself when selected
        // Set sphere color based on waypoint type
        if (type == WaypointType.Roaming)
            Gizmos.color = Color.blue;
        else
            Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.15f); // Slightly larger sphere to indicate selection

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