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
    #region Fields
    // Array of submenus to be toggled
    public GameObject[] optionsMenus;

    // Audio option initialization
    [Header("Audio Section")]
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
    [Header("General Section")]
    [SerializeField] private Toggle subtitleCheckbox;
    [SerializeField] private Slider sensitivitySliderX;
    [SerializeField] private Slider sensitivitySliderY;
    [SerializeField] private TextMeshProUGUI sensitivityXSliderText;
    [SerializeField] private TextMeshProUGUI sensitivityYSliderText;
    [SerializeField] private ZeroGravity player = null;
    [SerializeField] private GameObject dialogueText = null;

    // Video option
    [Header("Video Section")]
    Resolution[] resolutions;
    private List<int> commonHz = new List<int> { 60, 75, 120, 144, 165 };
    RefreshRate refresh;
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown refreshRateDropdown;
    [SerializeField] private TMP_Dropdown graphicsQuality;
    [SerializeField] private Toggle fullscreenCheckbox;

    // Camera option initialization
    [Header("Camera Section")]
    [SerializeField] private Slider fovSlider;
    [SerializeField] private Slider gammaSlider;
    [SerializeField] private Slider bloomSlider;
    [SerializeField] private TextMeshProUGUI fovSliderText;
    [SerializeField] private TextMeshProUGUI gammaSliderText;
    [SerializeField] private TextMeshProUGUI bloomSliderText;
    [SerializeField] private Volume postProcessing;


    // Menu manager variables
    [Header("Menu Manager")]
    [SerializeField] private MenuManager menuManager = null;
    [SerializeField] private GameObject confirmationPopUp;
    public bool isChanged;
    #endregion

    /// <summary> 
    /// Runs the setup method when the game object is enabled in the scene
    /// </summary>
    private void OnEnable()
    {
        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        GetResolutions();
        SetUp();
    }

    // Setting methods
    #region Setters
    public void SetUp()
    {
        // Makes sure menus are not open when starting 
        CloseMenus();
        graphicsQuality.value = GetPrefs("qualityLevel",5);
        fullscreenCheckbox.isOn = GetPrefs("isFullscreen", 1) == 1;
        ResetValues();
        ApplyOptions();
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
        resolutionDropdown.value = resolutionIndex;
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, FullScreenMode.ExclusiveFullScreen, refresh);
    }

    public void SetRefreshRate(int refreshRate)
    {
        refreshRateDropdown.value = refreshRate;
        refresh = new RefreshRate { numerator = (uint)commonHz[refreshRateDropdown.value], denominator = 1 };
        Screen.SetResolution(Screen.width, Screen.height, FullScreenMode.ExclusiveFullScreen, refresh);
        PopulateResolutionDropdown();
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

    public void SetFOV(float Value)
    {
        fovSlider.value = Value;
        if (player != null)
        {
            player.cam.fieldOfView = Value;
        }
        fovSliderText.text = fovSlider.value.ToString();
        isChanged = true;
    }
    public void SetGamma(float Value)
    {
        gammaSlider.value = Value;
        if (player != null)
        {
            if(postProcessing != null && postProcessing.profile.TryGet<LiftGammaGain>(out LiftGammaGain liftGammaGain))
            {
                liftGammaGain.gamma.Override(new Vector4(1f, 1f, 1f, Value));
            }

        }
        gammaSliderText.text = (gammaSlider.value*10).ToString("N0");
    }
    public void SetBloom(float Value)
    {
        bloomSlider.value = Value;
        if (postProcessing!=null && postProcessing.profile.TryGet<Bloom>(out Bloom bloom))
        {
            bloom.intensity.value = Value;
        }
        bloomSliderText.text = (bloomSlider.value*10).ToString("N0");
        isChanged = true;
    }

    #endregion
    // Menu management
    #region Popups
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
    /// <summary>
    /// Closes options menu or pulls up confirmation page if options are changed
    /// </summary>
    public void ExitOptions()
    {
        if (isChanged)
        {
            OpenPopUp(confirmationPopUp);
        }
        else
        {
            menuManager.LastMenu();
        }
    }
    #endregion
    // Helper Methods
    #region Helpers
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
    public void SetSliderText(TextMeshProUGUI sliderText, Slider volumeSlider)
    {
        sliderText.text = (volumeSlider.value * 100).ToString("N0");
        isChanged = true;
    }
    /// <summary>
    /// Gets all possible Resolutions for current displayed monitor
    /// </summary>
    private void GetResolutions()
    {
        PopulateRefreshRateDropdown();
        PopulateResolutionDropdown();
    }

    void PopulateRefreshRateDropdown()
    {
        refreshRateDropdown.ClearOptions();

        List<string> hzOptions = new List<string>();
        foreach (int hz in commonHz)
            hzOptions.Add(hz + " Hz");

        refreshRateDropdown.AddOptions(hzOptions);
    }

    void PopulateResolutionDropdown()
    {
        resolutionDropdown.ClearOptions();
        resolutions = Screen.resolutions;

        List<string> resOptions = new List<string>();
        List<Resolution> uniqueResolutions = new List<Resolution>();

        foreach (Resolution res in resolutions)
        {
            // Avoid duplicates
            if (!uniqueResolutions.Exists(r => r.width == res.width && r.height == res.height))
                uniqueResolutions.Add(res);
        }
        resolutions = uniqueResolutions.ToArray();
        resolutionDropdown.value = resolutions.Length-1;
        foreach (var res in uniqueResolutions)
            resOptions.Add(res.width + " x " + res.height);
        
        resolutionDropdown.AddOptions(resOptions);
    }
    #endregion
    // Persistant options
    #region Preferences
    /// <summary>
    /// Sets the preferences of all the options
    /// </summary>
    void SetAllPrefs()
    {
        SetPrefsFloat("dialogueSlider", dialogueSlider.value);
        SetPrefsFloat("soundFXSlider", soundFXSlider.value);
        SetPrefsFloat("musicSlider", musicSlider.value);
        SetPrefsFloat("masterVolumeSlider", masterVolumeSlider.value);
        SetPrefsInt("sensitivitySliderX", (int)sensitivitySliderX.value);
        SetPrefsInt("sensitivitySliderY", (int)sensitivitySliderY.value);
        SetPrefsInt("refreshRate", refreshRateDropdown.value);
        SetPrefsInt("resolution", resolutionDropdown.value);
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

    /// <summary>
    /// Takes saved player preferences and applies to all options
    /// </summary>
    public void ResetValues()
    {
        SetDialogueVolume(GetPrefsFloat("dialogueSlider", 1f));
        SetSoundFXVolume(GetPrefsFloat("soundFXSlider", 1f));
        SetSensitivityX(GetPrefs("sensitivitySliderX", 40));
        SetSensitivityY(GetPrefs("sensitivitySliderY", 40));
        SetMusicVolume(GetPrefsFloat("musicSlider", 0.5f));
        SetMasterVolume(GetPrefsFloat("masterVolumeSlider", 1f));
        SetRefreshRate(GetPrefs("refreshRate", 2));
        SetResolution(GetPrefs("resolution",resolutions.Length-1));
        SetFOV(GetPrefsFloat("fovSlider", 112f));
        SetGamma(GetPrefsFloat("gammaSlider", 0f));
        SetBloom(GetPrefsFloat("bloomSlider", 0f));
        ToggleSubtitles();
    }
    #endregion
    
}
