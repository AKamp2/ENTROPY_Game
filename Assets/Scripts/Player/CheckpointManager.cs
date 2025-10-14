using UnityEngine;
using System.Collections.Generic;

public class CheckpointManager : MonoBehaviour, ISaveable
{
    [Tooltip("Ordered list of your checkpoints in scene")]
    [SerializeField]
    public List<Checkpoint> checkpoints;
    public ZeroGravity playerZeroG;    // drag your player’s ZeroGravity component here

    int _currentIndex = 0;

    void Start()
    {
        // Wire up each checkpoint and only enable the first one
        for (int i = 0; i < checkpoints.Count; i++)
        {
            var cp = checkpoints[i];
            cp.OnReached += HandleCheckpointReached;
            cp.Initialize(playerZeroG, i == 0);
        }
    }

    void HandleCheckpointReached(Checkpoint reached)
    {
        // advance to next checkpoint if there is one
        if (_currentIndex + 1 < checkpoints.Count)
        {
            _currentIndex++;
            checkpoints[_currentIndex].Initialize(playerZeroG, true);
        }
        // store the Player's data to the save manager, passing in the position of this checkpoint
        playerZeroG.StorePlayerData(reached.respawnPoint.transform.position);
        // save the game at checkpoints
        GlobalSaveManager.SavedWithTerminal = false;
        GlobalSaveManager.SaveGame(false);
    }

    public void LoadSaveFile(string fileName)
    {
        // this will load data from the file to a variable we will use to change this objects data
        string path = Application.persistentDataPath;
        string loadedData = GlobalSaveManager.LoadTextFromFile(path, fileName);
        bool[] _checkpointStates = JsonUtility.FromJson<bool[]>(loadedData);
        for (int i = 0; i < checkpoints.Count; i++)
        {
            checkpoints[i].Col.enabled = _checkpointStates[i];
        }
    }

    public void CreateSaveFile(string fileName)
    {
        // store a copy of the checkpoint data in the global save manager
        // GlobalSaveManager.Instance.Data.Checkpoints = new List<Checkpoint>(checkpoints);
        bool[] _checkPointStates = new bool[checkpoints.Count];
        for (int i = 0; i < checkpoints.Count; i++)
        {
            _checkPointStates[i] = checkpoints[i].Col.enabled;
        }
        // this will create a file backing up the data we give it
        string json = JsonUtility.ToJson(_checkPointStates);
        string path = Application.persistentDataPath;
        GlobalSaveManager.SaveTextToFile(path, fileName, json);
    }
}
