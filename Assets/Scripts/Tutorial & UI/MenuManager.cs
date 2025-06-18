using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
//using static UnityEditor.SceneView;

public class MenuManager : MonoBehaviour
{
    // Serialize Menus for managing
    [SerializeField] private GameObject deathMenu;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject thanksMenu;
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private GameObject optionsCanvas;
    public List<GameObject> activeMenus;

    // Serialize Player info for checkpoints
    [SerializeField] private ZeroGravity player;
    [SerializeField] private GameObject respawnLoc;
    [SerializeField] private GameObject playerCanvas;
    [SerializeField] private CameraFade cameraFade;
    public static bool playerDead = false;

    // Audio Manager
    [SerializeField] private AudioSource dialogue;

    // Win Condition
    [SerializeField]
    private Win winScript;

    // Set options
    [SerializeField] SettingsMenu SettingsMenu;

    public void Start()
    {
        activeMenus = new List<GameObject>();
        SettingsMenu.ApplyOptions();
    }
    private void Update()
    {
        if (activeMenus.Count == 0 && deathMenu.activeInHierarchy)
        {
            activeMenus.Add(deathMenu);
        }
        if (activeMenus.Count == 0 && pauseMenu.activeInHierarchy)
        {
            activeMenus.Add(pauseMenu);
            
        }
        if (player.IsDead)
        {
            Dead();
        }
        if (winScript.WinCondition && activeMenus.Count== 0)
        {
            activeMenus.Add(thanksMenu);
            thanksMenu.SetActive(true);
        }
        if (activeMenus.Count > 0)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            playerCanvas.SetActive(false);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            playerCanvas.SetActive(true);
        }
    }
    public void Pause()
    {
        if(activeMenus.Count == 0)
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0;
            dialogue.Pause();
        }
    }

    public void Resume()
    {
        Time.timeScale = 1;
        CloseMenus();
        dialogue.UnPause();
    }

    public void OptionsButton()
    {
        optionsMenu.SetActive(true);
        optionsCanvas.SetActive(true);
    }

    public void LastCheckpoint()
    {
        // Need state machine for player to be reset
        Debug.Log("Load Last Checkpoint selected");
        Resume();
        
        Time.timeScale = 1f; // Ensure normal game speed
        playerDead = false;

        StartCoroutine(cameraFade.FadeIn(1.5f));
        player.Respawn();
        playerCanvas.SetActive(true);  // Re-enable player UI
    }

    public void LoadMenu()
    {
        Debug.Log("Menu selected");
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void Dead()
    {
        //restrict the player from movement
        player.PlayerDead();
        //set the death menu to true.
        deathMenu.SetActive(true);
        dialogue.Pause();
    }

    public void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void LastMenu()
    {
        if (activeMenus.Count > 0) {
            activeMenus[0].SetActive(true);
            optionsMenu.SetActive(false);
            activeMenus.Clear();
        }
    }

    void CloseMenus()
    {
        foreach (var menu in activeMenus)
        {
            menu.SetActive(false);
        }
        activeMenus.Clear();
    }
}
