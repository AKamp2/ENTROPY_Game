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
    [SerializeField] private AudioSource soundFXAudioSource;
    [SerializeField] private AudioSource dialogueAudioSource;

    // General setting initialization
    [SerializeField] private Toggle subtitleCheckbox;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private ZeroGravity player;
    [SerializeField] private GameObject dialogueText;

    private void OnEnable()
    {
        // Makes sure menus are not open when starting 
        CloseMenus();
        dialogueSlider.value = GetPrefs("dialogueSlider", 1);
        soundFXSlider.value = GetPrefs("soundFXSlider", 1);
        sensitivitySlider.value = GetPrefs("sensitivitySlider", 4);
        subtitleCheckbox.isOn = GetPrefs("subtitleCheckbox", 1) == 1;
        ApplySettings();
    }
   
    public void SetDialogueVolume()
    {
        // Sets the dialogue volume to the slide value
        dialogueAudioSource.volume = dialogueSlider.value;
        //Debug.Log(audioSource.volume);
        SetPrefs("dialogueSlider", dialogueSlider.value);
    }

    public void SetSoundFXVolume()
    {
        // Sets the sound effects volume to the slide value
        soundFXAudioSource.volume = soundFXSlider.value;
        //Debug.Log(audioSource.volume);
        SetPrefs("soundFXSlider", soundFXSlider.value);
    }

    public void SetSensitivity()
    {
        // Sets the sound effects volume to the slide value
        player.SensitivityX = sensitivitySlider.value;
        player.SensitivityY = sensitivitySlider.value;
        SetPrefs("sensitivitySlider", sensitivitySlider.value);
    }

    public void ToggleSubtitles()
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

    public void OpenPopUp(GameObject popUp)
    {
        popUp.SetActive(true);
    }

    public void ClosePopUp(GameObject popUp)
    {
        popUp.SetActive(false);
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

    public void ApplySettings()
    {
        ToggleSubtitles();
        SetDialogueVolume();
        SetSoundFXVolume();
        SetSensitivity();
    }
}
