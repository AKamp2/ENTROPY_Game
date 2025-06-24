using UnityEngine;
using UnityEngine.InputSystem;

public class LockdownEvent : MonoBehaviour
{
    [SerializeField]
    private BoxCollider DoorTrigger;
    [SerializeField]
    private GameObject lever;
    [SerializeField]
    private DoorScript[] doors;

    // lockdown bools
    private bool isActive;
    private bool canPull;

    // wrist monitor
    private bool canGrab;
    private bool isGrabbable;

    public bool CanPull
    {
        get { return canPull; }
        set { canPull = value; }
    }

    public bool IsActive
    {
        get { return isActive; }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DoorTrigger.enabled = false;
        // checks if player is currently hovering over lever
        canPull = false;
        // checks if system is able to be turned on
        isActive = true;

        canGrab = false;
        isGrabbable = true;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenDoors()
    {
        foreach (DoorScript door in doors)
        {
            door.DoorState = DoorScript.States.Opening;
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (canPull && isActive)
        {
            DoorTrigger.enabled = true;
            isActive = false;

            // begin lighting and audio queues
        }
    }




}
