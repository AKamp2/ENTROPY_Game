using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

public class PickupScript : MonoBehaviour
{
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private Transform holdPos;
    [SerializeField]
    private Camera cam;

    [SerializeField]
    private GameObject ObjectContainer;

    private bool canPickUp = false;

    [SerializeField]
    private float throwForce = 500f; //force at which the object is thrown at
    [SerializeField]
    private float pickUpRange = 4f; //how far the player can pickup the object from
    private GameObject heldObj; //object which we pick up
    private Rigidbody heldObjRb; //rigidbody of object we pick up

    [SerializeField]
    private Collider playerCollider;

    private GameObject current;


    public bool hasThrownObject = false; //for tutorial section for detecting throwing

    // Start is called before the first frame update
    void Start()
    {
            
    }

    // Update is called once per frame
    void Update()
    {
        if (heldObj == null) //if currently not holding anything
        {
            //perform raycast to check if player is looking at object within pickuprange
            RaycastHit hit;
            if (Physics.Raycast(cam.transform.position, cam.transform.TransformDirection(Vector3.forward), out hit, pickUpRange))
            {
                //make sure pickup tag is attached
                if (hit.transform.gameObject.tag == "PickupObject")
                {
                    current = hit.transform.gameObject;
                    canPickUp = true;
                }
            }
            else
            {
                current = null;
                canPickUp=false;
            }
        }


    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        //if (buttonPressed)
        //{
        //    buttonPressed = false;
        //}
        //else
        //{
        //    buttonPressed = true;
        //}

        if (!context.performed) return;

        if (canPickUp && heldObj == null)
        {
            PickUpObject(current);
            Debug.Log("Picked up object");
        }
        else if (heldObj != null)
        {
            Debug.Log("Dropped object");
            DropObject();
        }

    }

    public void OnThrow(InputAction.CallbackContext context)
    {
        if (heldObj != null) //if player is holding object
        {
            MoveObject(); //keep object position at holdPos
            ThrowObject();

        }
     
    }

    void PickUpObject(GameObject pickUpObj)
    {
        if (pickUpObj.GetComponent<Rigidbody>()) //make sure the object has a RigidBody
        {
            heldObj = pickUpObj; //assign heldObj to the object that was hit by the raycast (no longer == null)
            heldObjRb = pickUpObj.GetComponent<Rigidbody>(); //assign Rigidbody
            heldObjRb.isKinematic = true;
            heldObjRb.transform.parent = holdPos.transform; //parent object to holdposition
            heldObj.layer = 8; //change the object layer to the holdLayer
            //make sure object doesnt collide with player, it can cause weird bugs
            Physics.IgnoreCollision(heldObj.GetComponent<Collider>(), playerCollider, true);

            MoveObject();
        }
    }
    void DropObject()
    {
        Debug.Log(heldObj);
        //re-enable collision with player
        Physics.IgnoreCollision(heldObj.GetComponent<Collider>(), playerCollider, false);
        heldObj.layer = 0; //object assigned back to default layer
        heldObjRb.isKinematic = false;
        heldObj.transform.parent = ObjectContainer.transform; //unparent object
        heldObj = null; //undefine game object
        //current = null;
    }
    void MoveObject()
    {
        //keep object position the same as the holdPosition position
        heldObj.transform.position = holdPos.transform.position;
    }
  
    void ThrowObject()
    {
        //same as drop function, but add force to object before undefining it
        Physics.IgnoreCollision(heldObj.GetComponent<Collider>(), playerCollider, false);
        heldObj.layer = 0;
        heldObjRb.isKinematic = false;
        heldObj.transform.parent = ObjectContainer.transform;
        heldObjRb.AddForce(cam.transform.forward.normalized * throwForce);
        heldObj = null;
        hasThrownObject = true;

        transform.GetComponent<Rigidbody>().AddForce(-cam.transform.forward.normalized * (throwForce * (heldObjRb.mass * 0.5f)));

    }

}
