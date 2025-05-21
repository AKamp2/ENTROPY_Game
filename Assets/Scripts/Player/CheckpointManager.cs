using UnityEngine;
using System.Collections.Generic;

public class CheckpointManager : MonoBehaviour
{
    [Tooltip("Ordered list of your checkpoints in scene")]
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
    }
}
