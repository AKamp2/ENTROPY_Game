using System;
using System.Collections.Generic;
using UnityEngine; 

// Used by the GlobalSaveManager to store all the data that will go in the save files
[Serializable]
public struct SaveData
{
    // the data that will be saved
    //int mapIndex;
    [SerializeField]
    private PlayerData playerData;
    public PlayerData PlayerData
    {
        get { return playerData; }
        set { playerData = value; }
    }
    [SerializeField]
    private bool[] checkpointStates;
    public bool[] CheckpointStates
    {
        get { return checkpointStates; }
        set { checkpointStates = value; }
    }
    [SerializeField]
    private DoorScript.States[] doorStates;
    public DoorScript.States[] DoorStates
    {
        get { return doorStates; }
        set { doorStates = value; }
    }
}
