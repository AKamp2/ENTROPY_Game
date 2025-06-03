using UnityEngine;

public class GizmoTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, transform.forward * 1.0f);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, -transform.up * 1.0f);
    }

}
