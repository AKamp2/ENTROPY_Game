using System;
using System.Collections.Generic;
using UnityEngine; 

// Used by the GlobalSaveManager to store all the data that will go in the save files
[Serializable]
public class SaveData
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
    [SerializeField]
    private bool[] terminalStates;
    public bool[] TerminalStates
    {
        get { return terminalStates;}
        set { terminalStates = value; }
    }
    [SerializeField]
    private bool savedWithTerminal;
    public bool SavedWithTerminal
    {
        get { return savedWithTerminal; }
        set { savedWithTerminal = value; }
    }
    [SerializeField]
    private int latestTerminalIndex = -1;
    public int LatestTerminalIndex
    {
        get { return latestTerminalIndex; }
        set { latestTerminalIndex = value; }
    }
}
