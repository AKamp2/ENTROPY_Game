using System;
using System.Collections.Generic;
using UnityEngine; 

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
    private List<Checkpoint> checkpoints;
    public List<Checkpoint> Checkpoints
    {
        get { return checkpoints; }
        set { checkpoints = value; }
    }
    //private DoorScript[] doors;
    //public DoorScript[] Doors
    //{
    //    get { return doors; }
    //    set { doors = value; }
    //}
}
