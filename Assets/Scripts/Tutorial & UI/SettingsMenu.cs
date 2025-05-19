using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class SettingsMenu : MonoBehaviour 
{
    public GameObject[] optionsMenus;

    private void Awake()
    {
       CloseMenus();
    }

    // Click handlers
    public void OpenMenu(GameObject menu)
    {
        CloseMenus();
        menu.SetActive(true);
    }
    public void CloseMenus()
    {
        foreach (GameObject menu in optionsMenus)
        {
            menu.SetActive(false);
        }
    }
}
