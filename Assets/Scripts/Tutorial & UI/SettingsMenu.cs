using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour 
{
    public GameObject[] optionsMenus;
    
    // Audio setting initialization
    [SerializeField] private Slider dialogueSlider;
    [SerializeField] private Slider soundFXSlider;


    private void Awake()
    {
       // Makes sure menus are not open when starting 
       CloseMenus();
       
    }

    public void SetDialogueVolume(AudioSource audioSource)
    {
        audioSource.volume = dialogueSlider.value;
        Debug.Log(audioSource.volume);
    }

    public void SetSoundFXVolume(AudioSource audioSource)
    {
        audioSource.volume = soundFXSlider.value;
        Debug.Log(audioSource.volume);

    }

    // Click handlers
    public void OpenMenu(GameObject menu)
    {
        // Closes other menus before opening
        // the menu that matches the section button
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
