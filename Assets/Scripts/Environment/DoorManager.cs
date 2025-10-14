using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class DoorManager : MonoBehaviour, ISaveable
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

    public void LoadSaveFile(string fileName)
    {
        // this will load data from the file to a variable we will use to change this objects data
        string path = Application.persistentDataPath;
        string loadedData = GlobalSaveManager.LoadTextFromFile(path, fileName);
        DoorScript.States[] _doorStates = JsonUtility.FromJson<DoorScript.States[]>(loadedData);
        for (int i = 0; i < doors.Length; i++)
        {
            doors[i].SetState(_doorStates[i]);
        }
    }

    public void CreateSaveFile(string fileName)
    {
        // store a copy of the checkpoint data in the global save manager
        DoorScript.States[] _doorStates = new DoorScript.States[doors.Length];
        for (int i = 0; i < doors.Length; i++)
        {
            _doorStates[i] = doors[i].DoorState;
        }
        // this will create a file backing up the data we give it
        string json = JsonUtility.ToJson(_doorStates);
        string path = Application.persistentDataPath;
        GlobalSaveManager.SaveTextToFile(path, fileName, json);
    }
}
