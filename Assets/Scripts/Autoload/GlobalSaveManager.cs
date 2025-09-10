using UnityEngine;
using System.Collections.Generic;

// this contains info about the game, such as the current level and its state
// this is the file the will handle saving and loading
public class GlobalSaveManager : MonoBehaviour
{
    public static GlobalSaveManager Instance { get; private set; }
    int mapIndex;
    private PlayerData playerData;
    private List<Checkpoint> checkpoints;
    private DoorScript[] doors;

    private void Awake()
    {
        // Ensure that there is only one GlobalSaveManager
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        } else
        {
            Instance = this;
        }
    }
    public void AddPlayerData()
    {
        this.playerData = new PlayerData();
    }
    public void AddCheckpoints(List<Checkpoint> _checkpoints)
    {
        checkpoints = _checkpoints;
    }
    public void AddDoors(DoorScript[] _doors)
    {
        doors = _doors;
    }
}
