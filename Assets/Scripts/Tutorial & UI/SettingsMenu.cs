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
using static UnityEngine.Rendering.DebugUI;

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
    [SerializeField] private AudioSource soundFXAudioSource = null;
    [SerializeField] private AudioSource dialogueAudioSource = null;
    [SerializeField] private TextMeshProUGUI musicSliderText;
    [SerializeField] private TextMeshProUGUI masterVolumeSliderText;
    [SerializeField] private AudioMixer masterVolumeMixer;

    // General option initialization
    [SerializeField] private Toggle subtitleCheckbox;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TextMeshProUGUI sensitivitySliderText;
    [SerializeField] private ZeroGravity player = null;
    [SerializeField] private GameObject dialogueText = null;

    // Menu manager variables
    [SerializeField] private MenuManager menuManager = null;
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
        sliderText.text = (musicSlider.value * 100).ToString("N0");
        isChanged = true;

    }
    public void SetSensitivitySliderText(TextMeshProUGUI sliderText)
    {
        sliderText.text = sensitivitySlider.value.ToString();
        isChanged = true;

    }
    public void SetMasterVolumeSliderText(TextMeshProUGUI sliderText)
    {
        sliderText.text = (masterVolumeSlider.value*100).ToString("N0");
        isChanged = true;
    }
    public void SetDialogueVolume()
    {
        // Sets the dialogue volume to the slide value
        if(dialogueAudioSource != null)
        {
            dialogueAudioSource.volume = dialogueSlider.value / 100;
        }
        //Debug.Log(audioSource.volume);
        SetPrefsInt("dialogueSlider", (int)dialogueSlider.value);
    }

    public void SetSoundFXVolume()
    {
        // Sets the sound effects volume to the slide value
        if (soundFXAudioSource != null)
        {
            soundFXAudioSource.volume = soundFXSlider.value / 100;
        }
        else
        {
            SetPrefsInt("soundFXSlider", (int)soundFXSlider.value);
        }
    }
    
    public void SetMusicVolume(float Value)
    {
        // Sets the sound effects volume to the slide value
        //Debug.Log(musicSlider.value);
        if(masterVolumeMixer != null)
        {
            masterVolumeMixer.SetFloat("MusicVolume", Mathf.Log10(Value) * 20);
        }
        //Debug.Log(audioSource.volume);
        SetPrefsFloat("musicSlider", Value);
    }

    public void SetMasterVolume(float Value)
    {
        // Sets the dialogue volume to the slide value
        masterVolumeMixer.SetFloat("MasterVolume",Mathf.Log10(Value) *20);
        Debug.Log(Mathf.Log10(Value) * 20);
        SetPrefsFloat("masterVolumeSlider", Value);
    }

    public void SetSensitivity()
    {
        // Sets the sound effects volume to the slide value
        if (player != null)
        {
            player.SensitivityX = sensitivitySlider.value / 10;
            player.SensitivityY = sensitivitySlider.value / 10;
        }
       
        SetPrefsInt("sensitivitySlider", (int)sensitivitySlider.value);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        if (isFullscreen)
        {
            SetPrefsInt("isFullscreen", 1);
        }
        if (!isFullscreen)
        {
            SetPrefsInt("isFullscreen", 0);
        }
    }

    public void SetGraphicsQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        SetPrefsInt("qualityLevel", qualityIndex);
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void ToggleSubtitles()
    {
        if (dialogueText != null)
        {
            if (subtitleCheckbox.isOn)
            {
                dialogueText.SetActive(true);
                SetPrefsInt("subtitleCheckbox", 1);
            }
            else
            {
                dialogueText.SetActive(false);
                SetPrefsInt("subtitleCheckbox", 0);
            }
            isChanged = true;
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

    void SetPrefsInt(string keyName, int value)
    {
        PlayerPrefs.SetInt(keyName, value);
    }
    void SetPrefsFloat(string keyName, float value)
    {
        PlayerPrefs.SetFloat(keyName, value);
    }
    int GetPrefs(string keyName, int defaultValue)
    {
        return PlayerPrefs.GetInt(keyName, defaultValue);
    }
    float GetPrefsFloat(string keyName, float defaultValue)
    {
        return PlayerPrefs.GetFloat(keyName, defaultValue);
    }
    public void ApplyOptions()
    {
        ToggleSubtitles();
        SetDialogueVolume();
        SetSoundFXVolume();
        SetSensitivity();
        SetMusicVolume(GetPrefsFloat("musicSlider", 0.5f));
        SetMasterVolume(GetPrefsFloat("masterVolumeSlider", 1));
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
