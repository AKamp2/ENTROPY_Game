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
    ZeroGravity zeroGPlayer;
    [SerializeField]
    PlayerUIManager uiManager;
    [SerializeField]
    private Transform holdPos;
    [SerializeField]
    private Camera cam;

    [SerializeField]
    private GameObject ObjectContainer;

    [SerializeField]
    private bool canPickUp = false;
    [SerializeField]
    private float coolDownMax = 3;
    [SerializeField]
    private float coolDown;


    [SerializeField]
    private LayerMask objectLayer;
    [SerializeField]
    private float throwForce = 5f; //force at which the object is thrown at
    [SerializeField]
    private float pickUpRange = 2f; //how far the player can pickup the object from
    private GameObject heldObj; //object which we pick up
    private Rigidbody heldObjRb; //rigidbody of object we pick up

    [SerializeField]
    private Collider playerCollider;
    private GameObject current;

    private bool hasThrownObject = false; //for tutorial section for detecting throwing

    public float PickUpRange
    {
        get { return pickUpRange; }
    }

    public LayerMask ObjectLayer
    {
        get { return objectLayer; } 
    }

    public bool HasThrownObject
    {
        get { return hasThrownObject; }
        set { hasThrownObject = value;  }
    }

    public bool CanPickUp
    {
        get { return canPickUp; }
        set { canPickUp = value; }
    }

    public GameObject HeldObject
    {
        get { return heldObj; }
    }

    // Start is called before the first frame update
    void Start()
    {
         coolDown = 0;
    }

    // Update is called once per frame
    void Update()
    {
       
        if (heldObj == null && zeroGPlayer.CanGrab) //if currently not holding anything and is allowed to grab things
        {
            //perform raycast to check if player is looking at object within pickuprange
            RaycastHit hit;
            float sphereRadius = 0.5f; // adjust for how wide you want the grab to be
            if (Physics.SphereCast(cam.transform.position, sphereRadius, cam.transform.forward, out hit, pickUpRange))
            {
                //make sure pickup tag is attached
                if (coolDown <= 0 && hit.transform.CompareTag("PickupObject"))
                {
                    current = hit.transform.gameObject;
                    canPickUp = true;

                    // activate the F key UI
                    // Removing the UI is handled by ZeroGPlayer
                    uiManager.InputIndicator.sprite = uiManager.KeyFIndicator;
                    uiManager.InputIndicator.color = new Color(256, 256, 256, 0.5f);
                }
            }
            else
            {
                current = null;
                canPickUp = false;
            }
        }

        if (coolDown > 0)
        {
            coolDown -= Time.deltaTime;
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
            //Debug.Log("Picked up object");
        }
        else if (heldObj != null)
        {
            //Debug.Log("Dropped object");
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
            //heldObj.GetComponent<Collider>().enabled = false;


            MoveObject();
            //zeroGPlayer.MoveHandsTo(holdPos.GetChild(0).transform, null);
        }
    }
    void DropObject()
    {
        //re-enable collision with player
        Physics.IgnoreCollision(heldObj.GetComponent<Collider>(), playerCollider, false);
        //heldObj.GetComponent<Collider>().enabled = true;

        Debug.Log(objectLayer.value);
        heldObj.layer = 9; //object assigned back to default layer
        heldObjRb.isKinematic = false;
        heldObj.transform.parent = ObjectContainer.transform; //unparent object
        heldObj = null; //undefine game object

        //current = null;
        //zeroGPlayer.MoveHandsTo(null, null);
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
        //heldObj.GetComponent<Collider>().enabled = true;
        heldObj.layer = 9;
        heldObjRb.isKinematic = false;
        heldObj.transform.parent = ObjectContainer.transform;
        heldObjRb.AddForce(cam.transform.forward.normalized * throwForce, ForceMode.VelocityChange);
        heldObj = null;
        hasThrownObject = true;
        StartCoroutine(ResetThrowFlag());

        transform.GetComponent<Rigidbody>().AddForce(-cam.transform.forward.normalized * (throwForce * (heldObjRb.mass * 0.5f)), ForceMode.VelocityChange);
        //Debug.Log("Thrown at velocity: " + heldObjRb.linearVelocity.magnitude);

      

        // initiate pick up cd
        canPickUp = false;
        coolDown = coolDownMax;

        //zeroGPlayer.MoveHandsTo(null, null);
    }

    IEnumerator ResetThrowFlag()
    {
        yield return null; // Wait one frame
        hasThrownObject = false;
    }

}
