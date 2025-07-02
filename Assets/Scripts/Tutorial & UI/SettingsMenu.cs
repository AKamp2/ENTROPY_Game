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
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SettingsMenu : MonoBehaviour 
{
    // Array of submenus to be toggled
    public GameObject[] optionsMenus;

    // Audio option initialization
    [SerializeField] private Slider dialogueSlider;
    [SerializeField] private TextMeshProUGUI dialogueSliderText;
    [SerializeField] private Slider soundFXSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private TextMeshProUGUI soundFXSliderText;
    [SerializeField] private AudioSource soundFXAudioSource;
    [SerializeField] private AudioSource dialogueAudioSource;
    [SerializeField] private AmbientController ambientAudioController;
    [SerializeField] private TextMeshProUGUI musicSliderText;
    [SerializeField] private TextMeshProUGUI masterVolumeSliderText;
    [SerializeField] private AudioMixerGroup masterVolume;

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

    // Video option
    Resolution[] resolutions;
    public TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown graphicsQuality;
    [SerializeField] private Toggle fullscreenCheckbox;


    private void OnEnable()
    {
        SetUp();
        
    }

    private void Start()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();

        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetUp()
    {
        // Makes sure menus are not open when starting 
        CloseMenus();
        dialogueSlider.value = GetPrefs("dialogueSlider", 100);
        soundFXSlider.value = GetPrefs("soundFXSlider", 100);
        musicSlider.value = GetPrefs("musicSlider", 100);
        masterVolumeSlider.value = GetPrefs("masterVolumeSlider", 100);
        sensitivitySlider.value = GetPrefs("sensitivitySlider", 40);
        subtitleCheckbox.isOn = GetPrefs("subtitleCheckbox", 1) == 1;
        graphicsQuality.value = GetPrefs("qualityLevel",5);
        fullscreenCheckbox.isOn = GetPrefs("isFullscreen", 1) == 1;
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
    public void SetMasterVolumeSliderText(TextMeshProUGUI sliderText)
    {
        sliderText.text = masterVolumeSlider.value.ToString();
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
        Debug.Log(musicSlider.value);
        ambientAudioController.SetVolume(musicSlider.value/100);
        //Debug.Log(audioSource.volume);
        SetPrefs("musicSlider", (int)musicSlider.value);
    }

    public void SetMasterVolume()
    {
        // Sets the dialogue volume to the slide value
        masterVolume.audioMixer.SetFloat("attentuation",masterVolumeSlider.value - 100);
        //Debug.Log(audioSource.volume);
        SetPrefs("masterVolumeSlider", (int)masterVolumeSlider.value);
    }

    public void SetSensitivity()
    {
        // Sets the sound effects volume to the slide value
        player.SensitivityX = sensitivitySlider.value/10;
        player.SensitivityY = sensitivitySlider.value/10;
        SetPrefs("sensitivitySlider", (int)sensitivitySlider.value);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        if (isFullscreen)
        {
            SetPrefs("isFullscreen", 1);
        }
        if (!isFullscreen)
        {
            SetPrefs("isFullscreen", 0);
        }
    }

    public void SetGraphicsQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        SetPrefs("qualityLevel", qualityIndex);
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
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
        SetMasterVolume();
        SetDialogueSliderText(dialogueSliderText);
        SetSensitivitySliderText(sensitivitySliderText);
        SetSoundFXSliderText(soundFXSliderText);
        SetMusicSliderText(musicSliderText);
        SetMasterVolumeSliderText(masterVolumeSliderText);
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
