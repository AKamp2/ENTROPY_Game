using UnityEditor;
using UnityEngine;

public class NewPauseMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Pause Game
        //Time.timeScale = 0;
    }

    public void Resume()
    {
        Time.timeScale = 1;
        this.gameObject.SetActive(false);
    }

    public void OpenSettings(GameObject settingsMenu)
    {
        settingsMenu.SetActive(true);
        Debug.Log("Settings Pressed");
    }

    public void Quit()
    {
       Application.Quit();
    }
}
