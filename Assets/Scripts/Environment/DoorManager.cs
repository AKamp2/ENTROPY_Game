using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

        // when loading from save, overwrite the doors
        if (GlobalSaveManager.Instance.LoadFromSave)
        {
            LoadDoorStates(GlobalSaveManager.Instance.Data.DoorStates);
        }
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

    // backs up door states for saving
    public void StoreDoorStates()
    {
        // store a copy of the checkpoint data in the global save manager
        DoorScript.States[] _doorStates = new DoorScript.States[doors.Length];
        for (int i = 0; i < doors.Length; i++)
        {
            _doorStates[i] = doors[i].DoorState;
        }
        GlobalSaveManager.Instance.Data.DoorStates = _doorStates;
    }

    // called when loading a save
    public void LoadDoorStates(DoorScript.States[] _doorStates)
    {
        for (int i = 0; i < doors.Length; i++)
        {
            doors[i].SetState(_doorStates[i]);
        }
    }
}
