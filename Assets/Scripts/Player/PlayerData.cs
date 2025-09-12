using System;
using UnityEngine;
// Stores the player's data for use in saving and loading
[Serializable]
public struct PlayerData
{
    [SerializeField]
    private Vector3 position;
    public Vector3 Position { get { return position; } }
    [SerializeField]
    private Quaternion rotation;
    public Quaternion Rotation { get { return rotation; } }
    [SerializeField]
    private int health;
    public int Health { get { return health; } }
    [SerializeField]
    private int stims;
    public int Stims { get { return stims; } }
    [SerializeField]
    private bool hasUsedStim;
    public bool HasUsedStim {  get { return hasUsedStim; } }
    [SerializeField]
    private bool inTutorial;
    public bool InTutorial { get { return inTutorial; } }
    public PlayerData(
        Vector3 _position,
        Quaternion _rotation,
        int _health, 
        int _stims, 
        bool _hasUsedStim, 
        bool _inTutorial
        )
    {
        position = _position;
        rotation = _rotation;
        health = _health;
        stims = _stims;
        hasUsedStim = _hasUsedStim;
        inTutorial = _inTutorial;
    }
}
