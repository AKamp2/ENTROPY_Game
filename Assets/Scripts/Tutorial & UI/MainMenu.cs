using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private SettingsMenu settingsMenu;
    [SerializeField] private GameObject confirmationPopUp;
    [SerializeField] private GameObject optionsMenu;

    private void Start()
    {
        // Ensure cursor is visible and locked
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Set time scale to normal
        Time.timeScale = 1f;
    }
    public void StartGame()
    {
        Debug.Log("sample");
        SceneManager.LoadScene("NewLevel");

    }
    public void Options()
    {
        optionsMenu.SetActive(true);
        settingsMenu.SetUp();
    }
    public void CloseOptions()
    {
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
