using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Audio;
using System;
using System.IO;
using TMPro;

public class SettingsMenu : MonoBehaviour 
{
    // Array of submenus to be toggled
    public GameObject[] optionsMenus;

    // Audio option initialization
    [SerializeField] private Slider dialogueSlider;
    [SerializeField] private TextMeshProUGUI dialogueSliderText;
    [SerializeField] private Slider soundFXSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TextMeshProUGUI soundFXSliderText;
    [SerializeField] private AudioSource soundFXAudioSource;
    [SerializeField] private AudioSource dialogueAudioSource;
    [SerializeField] private AmbientController ambientAudioController;

    // General option initialization
    [SerializeField] private Toggle subtitleCheckbox;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TextMeshProUGUI sensitivitySliderText;
    [SerializeField] private ZeroGravity player;
    [SerializeField] private GameObject dialogueText;

    // Menu manager variables
    [SerializeField] private MenuManager menuManager;
    [SerializeField] private GameObject confirmationPopUp;
    public bool isChanged;

    private void OnEnable()
    {
        SetUp();
    }

    public void SetUp()
    {
        // Makes sure menus are not open when starting 
        CloseMenus();
        dialogueSlider.value = GetPrefs("dialogueSlider", 1);
        soundFXSlider.value = GetPrefs("soundFXSlider", 1);
        sensitivitySlider.value = GetPrefs("sensitivitySlider", 4);
        subtitleCheckbox.isOn = GetPrefs("subtitleCheckbox", 1) == 1;
        ApplyOptions();
    }

    public void SetSoundFXSliderText(TextMeshProUGUI sliderText)
    {
        sliderText.text = soundFXSlider.value.ToString();
        isChanged = true;
    }
    public void SetDialogueSliderText(TextMeshProUGUI sliderText)
    {
        sliderText.text = dialogueSlider.value.ToString();
        isChanged = true;

    }
    public void SetMusicSliderText(TextMeshProUGUI sliderText)
    {
        sliderText.text = musicSlider.value.ToString();
        isChanged = true;

    }
    public void SetSensitivitySliderText(TextMeshProUGUI sliderText)
    {
        sliderText.text = sensitivitySlider.value.ToString();
        isChanged = true;

    }
    public void SetDialogueVolume()
    {
        // Sets the dialogue volume to the slide value
        dialogueAudioSource.volume = dialogueSlider.value/100;
        //Debug.Log(audioSource.volume);
        SetPrefs("dialogueSlider", (int)dialogueSlider.value);
    }

    public void SetSoundFXVolume()
    {
        // Sets the sound effects volume to the slide value
        soundFXAudioSource.volume = soundFXSlider.value/100;
        //Debug.Log(audioSource.volume);
        SetPrefs("soundFXSlider", (int)soundFXSlider.value);
    }
    
    public void SetMusicVolume()
    {
        // Sets the sound effects volume to the slide value
        ambientAudioController.SetVolume(musicSlider.value/100);
        //Debug.Log(audioSource.volume);
        SetPrefs("musicSlider", (int)musicSlider.value);
    }

    public void SetSensitivity()
    {
        // Sets the sound effects volume to the slide value
        player.SensitivityX = sensitivitySlider.value;
        player.SensitivityY = sensitivitySlider.value;
        SetPrefs("sensitivitySlider", (int)sensitivitySlider.value);
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
        isChanged = true;
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

    void SetPrefs(string keyName, int value)
    {
        PlayerPrefs.SetInt(keyName, value);
    }

    int GetPrefs(string keyName, int defaultValue)
    {
        return PlayerPrefs.GetInt(keyName, defaultValue);
    }

    public void ApplyOptions()
    {
        ToggleSubtitles();
        SetDialogueVolume();
        SetSoundFXVolume();
        SetSensitivity();
        SetMusicVolume();
        SetDialogueSliderText(dialogueSliderText);
        SetSensitivitySliderText(sensitivitySliderText);
        SetSoundFXSliderText(soundFXSliderText);
        isChanged = false;
    }

    public void ExitOptions()
    {
        if (isChanged) {
            OpenPopUp(confirmationPopUp);
        }
        else
        {
            menuManager.LastMenu();
        }
    }
}
