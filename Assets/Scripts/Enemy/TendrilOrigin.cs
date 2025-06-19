using UnityEngine;

[ExecuteAlways]
public class TendrilOrigin : MonoBehaviour
{

    public float gizmoLength = 1.0f;
    public Color gizmoColor = Color.cyan;

    public TendrilBehavior activeTendril;

    public bool CanSpawnTendril()
    {
        return activeTendril == null;
    }

    public void ClearTendril()
    {
        activeTendril = null;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Vector3 start = transform.position;
        Vector3 end = start + transform.forward * gizmoLength;

        // Draw the main line
        Gizmos.DrawLine(start, end);

        // Draw a small arrowhead
        Vector3 right = Quaternion.LookRotation(transform.forward) * Quaternion.Euler(0, 150, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(transform.forward) * Quaternion.Euler(0, -150, 0) * Vector3.forward;
        Gizmos.DrawLine(end, end + right * 0.3f);
        Gizmos.DrawLine(end, end + left * 0.3f);
    }

}
