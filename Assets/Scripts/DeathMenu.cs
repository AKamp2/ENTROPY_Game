using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class DeathMenu : Menu
{

    public void LastCheckpoint()
    {
        Debug.Log("Load Last Checkpoint selected");
    }

    public void OptionsButton()
    {
        Debug.Log("Options selected");
    }

    public void LoadMenu()
    {
        Debug.Log("Menu selected");
        isPaused = false;
        SceneManager.LoadScene("MainMenu");
    }

    public void Quit()
    {
        Debug.Log("Quit selected");
        Application.Quit();
    }
}

