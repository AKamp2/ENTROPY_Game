using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder.Shapes;

public class DoorManager : MonoBehaviour
{

    [SerializeField]
    ZeroGravity player;
    [SerializeField]
    private DoorScript[] doors;
    [SerializeField]
    private List<DoorScript> doorsInRange;

    public GameObject DoorUI = null;

    private GameObject currentSelectedDoor = null;

    [SerializeField]
    Material unlockedMaterial;
    [SerializeField]
    Material lockedMaterial;
    [SerializeField]
    Material brokenMaterial;

    [Header("Hologram Variables")]

    [SerializeField]
    public Texture2D lockedTexture;
    [SerializeField]
    public Color lockedColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
    [SerializeField]
    public Texture2D unlockedTexture;
    [SerializeField]
    public Color unlockedColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
    [SerializeField]
    public Texture2D warningTexture;
    [SerializeField]
    public Color warningColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);

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
        doorsInRange = new List<DoorScript>();

        // when loading from save, overwrite the doors
        if (GlobalSaveManager.Instance.LoadFromSave)
        {
            LoadDoorStates(GlobalSaveManager.Instance.Data.DoorStates);
        }
    }

    // Update is called once per frame
    void Update()
    {
        ScanDoors();
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

    private void ScanDoors()
    {
        Collider[] doorParts = Physics.OverlapSphere(player.transform.position, 5.0f, LayerMask.GetMask("Door"));
        List<DoorScript> nearDoors = new List<DoorScript>();

        // adds new doors in range
        foreach (Collider collider in doorParts)
        {

            DoorScript door = collider.transform.parent.GetComponentInParent<DoorScript>();
            nearDoors.Add(door);

            if (!doorsInRange.Contains(door))
            {
                MeshRenderer[] mr = collider.transform.parent.GetComponentsInChildren<MeshRenderer>();

                doorsInRange.Add(door);

                if (door.hologramGroup.Length != 0)
                {
                    Debug.Log("fade on");
                    // fade on
                    door.StartFade(0.0f, door.lightOn, 1.5f);

                }
                

            }


        }

        //clean out doors not in range
        for (int i = 0; i < doorsInRange.Count; i++)
        {
            {
                if (!nearDoors.Contains(doorsInRange[i]))
                {
                    //fade out doors no longer in range. checks for if the hologram is already deactivated from being open
                    if (doorsInRange[i].hologramGroup != null && doorsInRange[i].hologramActive == true)
                    {
                        doorsInRange[i].StartFade(1.0f, doorsInRange[i].lightOff, 1.5f);
                    }
                    
                    doorsInRange.Remove(doorsInRange[i]);
                    i--;
                }
            }

        }
    }

    public bool DoorInRange(DoorScript door)
    {

        if (doorsInRange.Contains(door)) return true;

        return false;

    }

    
}
