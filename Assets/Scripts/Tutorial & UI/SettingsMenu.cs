using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Audio;
using System;
using System.IO;

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
    [SerializeField] private GameObject subtitles;


    private void Awake()
    {
       // Makes sure menus are not open when starting 
       CloseMenus();
       dialogueSlider.value = GetPrefs("dialogueSlider",1);
       soundFXSlider.value = GetPrefs("soundFXSlider",1);
       sensitivitySlider.value = GetPrefs("sensitivitySlider",4);
       subtitleCheckbox.isOn = GetPrefs("subtitleCheckbox", 1) == 1;
        ToggleSubtitles(subtitles);
    }

    public void SetDialogueVolume(AudioSource audioSource)
    {
        // Sets the dialogue volume to the slide value
        audioSource.volume = dialogueSlider.value;
        //Debug.Log(audioSource.volume);
        SetPrefs("dialogueSlider", dialogueSlider.value);
    }

    public void SetSoundFXVolume(AudioSource audioSource)
    {
        // Sets the sound effects volume to the slide value
        audioSource.volume = soundFXSlider.value;
        //Debug.Log(audioSource.volume);
        SetPrefs("soundFXSlider", soundFXSlider.value);
    }

    public void SetSensitivity(ZeroGravity player)
    {
        // Sets the sound effects volume to the slide value
        player.SensitivityX = sensitivitySlider.value;
        player.SensitivityY = sensitivitySlider.value;
        SetPrefs("sensitivitySlider", sensitivitySlider.value);
    }

    public void ToggleSubtitles(GameObject dialogueText)
    {
        if (subtitleCheckbox.isOn)
        {
            dialogueText.SetActive(true);
            SetPrefs("subtitleCheckbox", 1);
        }
        else 
        { 
            dialogueText.SetActive(false);
            SetPrefs("subtitleCheckbox", 0);
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

    void SetPrefs(string keyName, float value)
    {
        int intValue = Mathf.RoundToInt(value);
        PlayerPrefs.SetInt(keyName, intValue);
    }

    int GetPrefs(string keyName, int defaultValue)
    {
        return PlayerPrefs.GetInt(keyName, defaultValue);
    }
}
