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
        SceneManager.LoadScene("SampleScene");
      
    }
    public void StartTestScene()
    {
        Debug.Log("Demo");
        SceneManager.LoadScene("DemoScene");
        
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
