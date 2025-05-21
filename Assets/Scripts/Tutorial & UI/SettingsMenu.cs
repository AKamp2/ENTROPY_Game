using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour 
{
    // Array of submenus to be toggled
    public GameObject[] optionsMenus;
    
    // Audio setting initialization
    [SerializeField] private Slider dialogueSlider;
    [SerializeField] private Slider soundFXSlider;

    // General setting initialization
    [SerializeField] private Toggle subtitleCheckbox;
    [SerializeField] private Slider sensitivitySlider;


    private void Awake()
    {
       // Makes sure menus are not open when starting 
       CloseMenus();
       
    }

    public void SetDialogueVolume(AudioSource audioSource)
    {
        // Sets the dialogue volume to the slide value
        audioSource.volume = dialogueSlider.value;
        Debug.Log(audioSource.volume);
    }

    public void SetSoundFXVolume(AudioSource audioSource)
    {
        // Sets the sound effects volume to the slide value
        audioSource.volume = soundFXSlider.value;
        Debug.Log(audioSource.volume);

    }

    public void SetSensitivity(ZeroGravity player)
    {
        // Sets the sound effects volume to the slide value
        player.SensitivityX = sensitivitySlider.value;
        player.SensitivityY = sensitivitySlider.value;
    }

    public void ToggleSubtitles(GameObject dialogueText)
    {
        if (subtitleCheckbox.isOn)
        {
            dialogueText.SetActive(true);
        }
        else 
        { 
            dialogueText.SetActive(false);
        }
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
