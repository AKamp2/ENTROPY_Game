using UnityEngine;
using UnityEngine.Rendering;

// this contains info about the game, such as the current level and its state
// this is the file the will handle saving and loading
public class GlobalSaveManager : MonoBehaviour
{
    public static GlobalSaveManager instance { get; private set; }

    private void Awake()
    {
        // Ensure that there is only one GlobalSaveManager
        if (instance != null && instance != this)
        {
            Destroy(this);
        } else
        {
            instance = this;
        }
    }

}
