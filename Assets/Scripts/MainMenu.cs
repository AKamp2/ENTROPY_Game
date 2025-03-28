using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
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

    public void QuitGame()
    {
        Application.Quit();
    }
}
