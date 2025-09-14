using UnityEngine;

public class GizmoTest : MonoBehaviour
{
    [SerializeField]
    Quaternion angle;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Vector3 object1Position = transform.position;
        //Vector3 object2Position = camera.transform.position;


        //float signedAngle = Vector3.Angle(object1Position, object2Position, transform.up);
        //angle = Quaternion.AngleAxis(signedAngle, transform.up);

        //Debug.Log(signedAngle);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, transform.forward * 1.0f);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, -transform.up * 1.0f);

       

        //if (transform.CompareTag("Grabbable"))
        //{
        //    Debug.Log(angle);
        //    Gizmos.DrawRay(transform.position, transform.rotation * angle.eulerAngles * 10);
        //}
    }

}
