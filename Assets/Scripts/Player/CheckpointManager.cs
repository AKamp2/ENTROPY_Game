using UnityEngine;
using System.Collections.Generic;

public class CheckpointManager : MonoBehaviour
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
        // when loading from save, overwrite the checkpoints
        if (GlobalSaveManager.Instance.LoadFromSave)
        {
            LoadCheckpointStates(GlobalSaveManager.Instance.Data.CheckpointStates);
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
        StoreCheckpointStates();
        playerZeroG.StorePlayerData();
        // save the game at checkpoints
        GlobalSaveManager.Instance.CreateSaveFile();
    }
    // backs up checkpoint states for saving
    public void StoreCheckpointStates()
    {
        // store a copy of the checkpoint data in the global save manager
        // GlobalSaveManager.Instance.Data.Checkpoints = new List<Checkpoint>(checkpoints);
        bool[] _checkPointStates = new bool[checkpoints.Count];
        for (int i = 0; i < checkpoints.Count; i++)
        {
            _checkPointStates[i] = checkpoints[i].Col.enabled;
        }
        GlobalSaveManager.Instance.Data.CheckpointStates = _checkPointStates;
    }

    // called when loading a save
    public void LoadCheckpointStates(bool[] _checkpointStates)
    {
        for (int i = 0; i < checkpoints.Count; i++)
        {
            checkpoints[i].Col.enabled = _checkpointStates[i];
        }
    }
}
