using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    private bool isPaused;
    private GameObject MenuUI;
    private PlayerController playerInput;

    public Menu()
    {
        this.isPaused = isPaused;
        this.playerInput = playerInput;
        this.MenuUI = MenuUI;
    }


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
