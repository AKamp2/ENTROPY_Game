using System.Collections;
using System.Collections.Generic;
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
    }
    public void StartGame()
    {
        Debug.Log("sample");
        // don't load from save, start a new game
        GlobalSaveManager.Instance.LoadFromSave = false;
        SceneManager.LoadScene("Level1New");

    }
    public void LoadGame()
    {
        GlobalSaveManager.Instance.LoadFromSave = true;
    }
    public void Options()
    {
        optionsMenu.SetActive(true);
        this.gameObject.SetActive(false);
        settingsMenu.SetUp();
        if (volume.profile.TryGet<LensDistortion>(out LensDistortion distortion))
        {
            distortion.active = false;
        }
    }
    public void CloseOptions()
    {
        
        if (settingsMenu.isChanged)
        {
            settingsMenu.OpenPopUp(confirmationPopUp);
        }
        else
        {
            if (volume.profile.TryGet<LensDistortion>(out LensDistortion distortion))
            {
                distortion.active = true;
            }
            optionsMenu.SetActive(false);
            this.gameObject.SetActive(true);
        }
    }
    public void ExitOptions()
    {
        if (volume.profile.TryGet<LensDistortion>(out LensDistortion distortion))
        {
            distortion.active = true;
        }
        optionsMenu.SetActive(false);
        this.gameObject.SetActive(true);
    }
    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
