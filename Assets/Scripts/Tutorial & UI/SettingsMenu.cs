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
    [SerializeField] private TextMeshProUGUI musicSliderText;
    [SerializeField] private TextMeshProUGUI masterVolumeSliderText;
    [SerializeField] private AudioMixer masterVolumeMixer;

    // General option initialization
    [SerializeField] private Toggle subtitleCheckbox;
    [SerializeField] private Slider sensitivitySliderX;
    [SerializeField] private Slider sensitivitySliderY;
    [SerializeField] private TextMeshProUGUI sensitivityXSliderText;
    [SerializeField] private TextMeshProUGUI sensitivityYSliderText;
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

    // Camera option initialization
    [SerializeField] private Slider fovSlider;
    [SerializeField] private Slider gammaSlider;
    [SerializeField] private Slider bloomSlider;
    [SerializeField] private TextMeshProUGUI fovSliderText;
    [SerializeField] private TextMeshProUGUI gammaSliderText;
    [SerializeField] private TextMeshProUGUI bloomSliderText;

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
        graphicsQuality.value = GetPrefs("qualityLevel",5);
        fullscreenCheckbox.isOn = GetPrefs("isFullscreen", 1) == 1;
        ResetValues();
        ApplyOptions();
    }

    public void SetSliderText(TextMeshProUGUI sliderText, Slider volumeSlider)
    {
        sliderText.text = (volumeSlider.value * 100).ToString("N0");
        isChanged = true;
    }

    public void SetDialogueVolume(float Value)
    {
        dialogueSlider.value = Value;
        if (masterVolumeMixer != null)
        {
            masterVolumeMixer.SetFloat("DialogueVolume", Mathf.Log10(Value) * 20);
        }
        //Debug.Log(audioSource.volume);
        SetSliderText(dialogueSliderText, dialogueSlider);
    }

    public void SetSoundFXVolume(float Value)
    {
        soundFXSlider.value = Value;
        if (masterVolumeMixer != null)
        {
            masterVolumeMixer.SetFloat("SoundFXVolume", Mathf.Log10(Value) * 20);
        }
        //Debug.Log(audioSource.volume);
        SetSliderText(soundFXSliderText, soundFXSlider);
    }

    public void SetMusicVolume(float Value)
    {
        musicSlider.value = Value;
        // Sets the sound effects volume to the slide value
        //Debug.Log(musicSlider.value);
        if (masterVolumeMixer != null)
        {
            masterVolumeMixer.SetFloat("MusicVolume", Mathf.Log10(Value) * 20);
        }
        //Debug.Log(audioSource.volume);
        SetSliderText(musicSliderText, musicSlider);
    }

    public void SetMasterVolume(float Value)
    {
        masterVolumeSlider.value = Value;
        // Sets the dialogue volume to the slide value
        if (masterVolumeMixer != null)
        {
            masterVolumeMixer.SetFloat("MasterVolume", Mathf.Log10(Value) * 20);
        }
        SetSliderText(masterVolumeSliderText, masterVolumeSlider);
    }

    public void SetSensitivityX(float Value)
    {
        sensitivitySliderX.value = Value;
        // Sets the sound effects volume to the slide value
        if (player != null)
        {
            player.SensitivityX = Value / 5f;
        }
        sensitivityXSliderText.text = Value.ToString();
        isChanged = true;
    }
    public void SetSensitivityY(float Value)
    {
        sensitivitySliderY.value = Value;
        // Sets the sound effects volume to the slide value
        if (player != null)
        {
            player.SensitivityY = Value / 10f;
        }
        sensitivityYSliderText.text = Value.ToString();
        isChanged = true;
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
            }
            else
            {
                dialogueText.SetActive(false);
                SetPrefsInt("subtitleCheckbox", 0);
            }
            isChanged = true;
        }
    }

    public void SetFOV(float Value)
    {
        fovSlider.value = Value;
        if (player != null)
        {
            player.cam.fieldOfView = fovSlider.value;
        }
        SetSliderText(fovSliderText, fovSlider);
    }
    public void SetGamma(float Value)
    {
        gammaSlider.value = Value;
        if (player != null)
        {
            
        }
        SetSliderText(gammaSliderText, gammaSlider);
    }
    public void SetBloom(float Value)
    {
        bloomSlider.value = Value;
        // volume.bloom = Value;
        SetSliderText(bloomSliderText, bloomSlider);
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
        SetAllPrefs();
        isChanged = false;
    }

    void SetAllPrefs()
    {
        SetPrefsFloat("dialogueSlider", dialogueSlider.value);
        SetPrefsFloat("soundFXSlider", soundFXSlider.value);
        SetPrefsFloat("musicSlider", musicSlider.value);
        SetPrefsFloat("masterVolumeSlider", masterVolumeSlider.value);
        SetPrefsInt("sensitivitySliderX", (int)sensitivitySliderX.value);
        SetPrefsInt("sensitivitySliderY", (int)sensitivitySliderY.value);
        SetPrefsFloat("fovSlider", fovSlider.value);
        SetPrefsFloat("gammaSlider", gammaSlider.value);
        SetPrefsFloat("bloomSlider", bloomSlider.value);
        if (subtitleCheckbox.isOn)
        {
            SetPrefsInt("subtitleCheckbox", 1);
        }
        else 
        { 
            SetPrefsInt("subtitleCheckbox", 0);
        }
    }

    public void ResetValues()
    {
        SetDialogueVolume(GetPrefsFloat("dialogueSlider", 1f));
        SetSoundFXVolume(GetPrefsFloat("soundFXSlider", 1f));
        SetSensitivityX(GetPrefs("sensitivitySliderX", 40));
        SetSensitivityY(GetPrefs("sensitivitySliderY", 40));
        SetMusicVolume(GetPrefsFloat("musicSlider", 0.5f));
        SetMasterVolume(GetPrefsFloat("masterVolumeSlider", 1f));
        SetFOV(GetPrefsFloat("fovSlider", 0f));
        SetGamma(GetPrefsFloat("gammaSlider", 0f));
        SetBloom(GetPrefsFloat("bloomSlider", 0f));
        ToggleSubtitles();
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
