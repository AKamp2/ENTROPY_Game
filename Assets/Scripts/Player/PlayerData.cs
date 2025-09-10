using UnityEngine;
// Stores the player's data for use in saving and loading
public struct PlayerData
{
    private int health;
    public int Health { get { return health; } }
    private int stims;
    public int Stims { get { return stims; } }
    private bool canGrab;
    public bool CanGrab { get { return canGrab; } }
    private bool canPropel;
    public bool CanPropel { get { return canPropel; } }
    private bool canPushoff;
    public bool CanPushoff { get { return canPushoff; } }
    private bool hasUsedStim;
    public bool HasUsedStim {  get { return hasUsedStim; } }
    private bool canRoll;
    public bool CanRoll { get { return canRoll; } }
    public PlayerData(
        int _health, 
        int _stims, 
        bool _canGrab, 
        bool _canPropel, 
        bool _canPushoff, 
        bool _hasUsedStim, 
        bool _canRoll
        )
    {
        health = _health;
        stims = _stims;
        canGrab = _canGrab;
        canPropel = _canPropel;
        canPushoff = _canPushoff;
        hasUsedStim = _hasUsedStim;
        canRoll = _canRoll;
    }
}
