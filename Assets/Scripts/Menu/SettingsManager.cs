//Farzana Tanni

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject controlsPopup; 
    public GameObject controlsButton;

    public Slider volumeSlider;
    public TMP_Dropdown graphicsDropdown; 
    public TMP_Dropdown resolutionDropdown; 

    private Resolution[] resolutions;

    private void Start()
    {

        if (controlsPopup != null)
        {
            controlsPopup.SetActive(false);
        }

        if (controlsButton != null)
        {
            controlsButton.SetActive(true);
        }

        InitializeResolutions();
        LoadSettings();
    }

    public void ShowControlsPopup()
    {
        controlsPopup.SetActive(true);
        if (controlsButton != null)
        {
            controlsButton.SetActive(false);
        }
    }

    public void HideControlsPopup()
    {
        controlsPopup.SetActive(false);
        if (controlsButton != null)
        {
            controlsButton.SetActive(true);
        }
    }

    private void InitializeResolutions()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        var resolutionOptions = new System.Collections.Generic.List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = $"{resolutions[i].width} x {resolutions[i].height}";
            resolutionOptions.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(resolutionOptions);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("Volume", volume);
    }

    public void SetGraphicsQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("GraphicsQuality", qualityIndex);
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution selectedResolution = resolutions[resolutionIndex];
        Screen.SetResolution(selectedResolution.width, selectedResolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt("Resolution", resolutionIndex);
    }

    public void LoadSettings()
    {
        if (PlayerPrefs.HasKey("Volume"))
        {
            float savedVolume = PlayerPrefs.GetFloat("Volume");
            volumeSlider.value = savedVolume;
            AudioListener.volume = savedVolume;
        }

        if (PlayerPrefs.HasKey("GraphicsQuality"))
        {
            int savedGraphics = PlayerPrefs.GetInt("GraphicsQuality");
            graphicsDropdown.value = savedGraphics;
            QualitySettings.SetQualityLevel(savedGraphics);
        }

        if (PlayerPrefs.HasKey("Resolution"))
        {
            int savedResolution = PlayerPrefs.GetInt("Resolution");
            resolutionDropdown.value = savedResolution;
            resolutionDropdown.RefreshShownValue();

            SetResolution(savedResolution);
        }
    }
}