using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DoorManager : MonoBehaviour
{

    [SerializeField]
    ZeroGravity player;
    [SerializeField]
    private DoorScript[] doors;

    public GameObject DoorUI = null;

    private GameObject currentSelectedDoor = null;

    [SerializeField]
    Material unlockedMaterial;
    [SerializeField]
    Material lockedMaterial;
    [SerializeField]
    Material brokenMaterial;

    public GameObject CurrentSelectedDoor
    {
        get { return currentSelectedDoor; }
        set { currentSelectedDoor = value; }
    }

    public Material UnlockedMaterial
    {
        get { return unlockedMaterial; }
    }

    public Material LockedMaterial
    { 
        get { return lockedMaterial; } 
    }

    public Material WarningMaterial
    {
        get { return brokenMaterial; }
    }

    // Start is called before the first frame update
    void Start()
    {
        doors = transform.Find("DoorGroup").GetComponentsInChildren<DoorScript>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        
        if (currentSelectedDoor)
        {
            DoorScript ds = currentSelectedDoor.GetComponent<DoorScript>();

            if (ds.HasPermissionLevel && player.AccessPermissions[(int)ds.Permission])
            {
                currentSelectedDoor.GetComponent<DoorScript>().UseDoor();
            }
            else if (!ds.HasPermissionLevel)
            {
                currentSelectedDoor.GetComponent<DoorScript>().UseDoor();
            }

                
        }
        
    }

}
