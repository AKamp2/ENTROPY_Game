using System;
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
    // these are for serialization and will be created during the save
    [Serializable]
    public class DoorData
    {
        [SerializeField]
        private List<DoorScript.States> doorStates;
        public List<DoorScript.States> DoorStates
        {
            get { return doorStates; }
        }
        public DoorData(List<DoorScript.States> _doorStates)
        {
            doorStates = _doorStates;
        }
    }
    public void LoadSaveFile(string fileName)
    {
        // this will load data from the file to a variable we will use to change this objects data
        string path = Application.persistentDataPath;
        string loadedData = GlobalSaveManager.LoadTextFromFile(path, fileName);
        DoorData _doorData = JsonUtility.FromJson<DoorData>(loadedData);
        for (int i = 0; i < _doorData.DoorStates.Count; i++)
        {
            doors[i].SetState(_doorData.DoorStates[i]);
        }
    }

    public void CreateSaveFile(string fileName)
    {
        // store a copy of the checkpoint data in the global save manager
        DoorData _doorData = new DoorData(new List<DoorScript.States>());
        foreach (DoorScript _door in doors)
        {
            _doorData.DoorStates.Add(_door.DoorState);
        }
        // this will create a file backing up the data we give it
        string json = JsonUtility.ToJson(_doorData);
        string path = Application.persistentDataPath;
        GlobalSaveManager.SaveTextToFile(path, fileName, json);
    }
}
