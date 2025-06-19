using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    public List<EnemySimpleAI> allEnemies = new List<EnemySimpleAI>();

    private void Awake()
    {
        // Singleton setup (optional)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Optionally, populate the list automatically at start
        //allEnemies.AddRange(FindObjectsByType<EnemySimpleAI>(FindObjectsSortMode.None));
    }

    /// <summary>
    /// Resets all enemies in the scene to their initial state.
    /// </summary>
    public void ResetAliens()
    {
        foreach (EnemySimpleAI alien in allEnemies)
        {
            if (alien != null)
                alien.ResetToStart();
        }
    }
}