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
        // Ensure cursor is visible and locked
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (volume.profile.TryGet<LensDistortion>(out LensDistortion distortion))
        {
            distortion.active = true;
        }
        // Set time scale to normal
        Time.timeScale = 1f;
    }
    public void StartGame()
    {
        Debug.Log("sample");
        SceneManager.LoadScene("Level1Graybox");

    }
    public void Options()
    {
        optionsMenu.SetActive(true);
        settingsMenu.SetUp();
        if (volume.profile.TryGet<LensDistortion>(out LensDistortion distortion))
        {
            distortion.active = false;
        }
    }
    public void CloseOptions()
    {
        if (volume.profile.TryGet<LensDistortion>(out LensDistortion distortion))
        {
            distortion.active = true;
        }
        if (settingsMenu.isChanged)
        {
            settingsMenu.OpenPopUp(confirmationPopUp);
        }
        else
        {
            optionsMenu.SetActive(false);
        }
    }
    public void ExitOptions()
    {
        if (volume.profile.TryGet<LensDistortion>(out LensDistortion distortion))
        {
            distortion.active = true;
        }
        optionsMenu.SetActive(false);
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
