using System.Collections;
using System.Collections.Generic;
using System.Xml.XPath;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private SettingsMenu settingsMenu;
    [SerializeField] private GameObject confirmationPopUp;
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private Volume volume;
    [SerializeField] private UIAudioManager uiAudio;
    private bool continueButtonEnabled = false;

    private void Start()
    {
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        // Ensure cursor is visible and locked
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (volume.profile.TryGet<LensDistortion>(out LensDistortion distortion))
        {
            distortion.active = true;
        }
        // Set time scale to normal
        Time.timeScale = 1f;
        //if(//GlobalSaveManager.Instance.Data.PlayerData. == 0)
        //{

        //}
        // show continue button
        if (GlobalSaveManager.SaveDataExists())
        {
            continueButtonEnabled = true;
        }
    }

    /// <summary>
    /// Loads a new game
    /// </summary>
    public void StartGame()
    {
        uiAudio?.PlaySelectSound();
        GlobalSaveManager.LoadFromSave = false;
        GlobalSaveManager.DeleteTempFiles();
        SceneManager.LoadScene("Level1New");
    }

    /// <summary>
    /// Loads saved instance of game
    /// </summary>
    public void LoadGame()
    {
        if (continueButtonEnabled)
        {
            uiAudio?.PlaySelectSound();
            GlobalSaveManager.LoadFromSave = true;
            GlobalSaveManager.DeleteTempFiles();
            GlobalSaveManager.OverwriteTempFiles();
            SceneManager.LoadScene("Level1New");
        } else
        {
            uiAudio?.PlayBackSound();
        }
    }

    /// <summary>
    /// Opens the options menu and turns off lens distortion
    /// </summary>
    public void Options()
    {
        uiAudio?.PlaySelectSound();
        optionsMenu.SetActive(true);
        this.gameObject.SetActive(false);
        settingsMenu.SetUp();
        if (volume.profile.TryGet<LensDistortion>(out LensDistortion distortion))
        {
            distortion.active = false;
        }
    }

    /// <summary>
    /// Prompts user to verify they want to leave options when something is changed
    /// </summary>
    public void CloseOptions()
    {
        uiAudio?.PlayBackSound();
        
        if (settingsMenu.isChanged)
        {
            settingsMenu.OpenPopUp(confirmationPopUp);
        }
        else
        {
            ExitOptions();
        }
    }

    /// <summary>
    /// Closes options menu
    /// </summary>
    public void ExitOptions()
    {
        uiAudio?.PlayBackSound();
        if (volume.profile.TryGet<LensDistortion>(out LensDistortion distortion))
        {
            distortion.active = true;
        }
        optionsMenu.SetActive(false);
        this.gameObject.SetActive(true);
    }

    /// <summary>
    /// Quits to desktop or stops playing if in editor
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
