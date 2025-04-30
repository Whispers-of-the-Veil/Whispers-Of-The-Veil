// Farzana Tanni

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

    [Header("UI Elements")]
    public GameObject settingsCanvas; 
    public GameObject controlsPopup;
    public GameObject controlsButton;
    public GameObject guidePopup;  
    public GameObject guideButton;  
    public Button closeSettingsButton;

    public Slider volumeSlider;
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown microphoneDropdown;

    private Resolution[] resolutions;
    public string selectedMicrophone;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (settingsCanvas != null)
            settingsCanvas.SetActive(false);
    }

    private void Start()
    {
        if (controlsPopup != null)
            controlsPopup.SetActive(false);

        if (controlsButton != null)
            controlsButton.SetActive(true);

        if (guidePopup != null)  
            guidePopup.SetActive(false);

        if (guideButton != null)  
            guideButton.SetActive(true);

        if (closeSettingsButton != null)
            closeSettingsButton.onClick.AddListener(CloseSettings);

        Debug.Log("Resolution Dropdown: " + (resolutionDropdown != null ? "Assigned" : "NULL"));
        Debug.Log("Microphone Dropdown: " + (microphoneDropdown != null ? "Assigned" : "NULL"));

        InitializeResolutions();
        InitializeMicrophones();
        LoadSettings();
        AdjustCamera();
    }

    public void OpenSettings()
    {
        Debug.Log("OpenSettings called");

        if (settingsCanvas != null)
        {
            settingsCanvas.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            Debug.LogWarning("settingsCanvas is NULL");
        }
    }

    public void CloseSettings()
    {
        if (settingsCanvas != null)
        {
            settingsCanvas.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    public void ShowControlsPopup()
    {
        if (controlsPopup != null)
        {
            controlsPopup.transform.SetAsLastSibling();
            controlsPopup.SetActive(true);
        }

        if (controlsButton != null)
            controlsButton.SetActive(false);
    }

    public void HideControlsPopup()
    {
        if (controlsPopup != null)
            controlsPopup.SetActive(false);

        if (controlsButton != null)
            controlsButton.SetActive(true);
    }

    public void ShowGuidePopup() 
    {
        if (guidePopup != null)
        {
            guidePopup.transform.SetAsLastSibling();
            guidePopup.SetActive(true);
        }

        if (guideButton != null)
            guideButton.SetActive(false);
    }

    public void HideGuidePopup() 
    {
        if (guidePopup != null)
            guidePopup.SetActive(false);

        if (guideButton != null)
            guideButton.SetActive(true);
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("Volume", volume);
    }

    private void InitializeResolutions()
    {
        if (resolutionDropdown == null)
        {
            Debug.LogWarning("resolutionDropdown is not assigned.");
            return;
        }

        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        var resolutionOptions = new System.Collections.Generic.List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = $"{resolutions[i].width} x {resolutions[i].height}";
            resolutionOptions.Add(option);

            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(resolutionOptions);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution selectedResolution = resolutions[resolutionIndex];
        Screen.SetResolution(selectedResolution.width, selectedResolution.height, Screen.fullScreenMode);

        PlayerPrefs.SetInt("Resolution", resolutionIndex);
        AdjustCamera();
    }

    private void InitializeMicrophones()
    {
        if (microphoneDropdown == null)
        {
            Debug.LogWarning("microphoneDropdown is not assigned.");
            return;
        }

        microphoneDropdown.ClearOptions();
        string[] devices = Microphone.devices;

        Debug.Log("Microphones found: " + devices.Length);

        if (devices.Length > 0)
        {
            microphoneDropdown.AddOptions(new System.Collections.Generic.List<string>(devices));
            selectedMicrophone = devices[0];
            microphoneDropdown.interactable = true;
        }
        else
        {
            microphoneDropdown.AddOptions(new System.Collections.Generic.List<string> { "No Microphone Found" });
            microphoneDropdown.interactable = false;
        }

        microphoneDropdown.onValueChanged.AddListener(OnMicrophoneChanged);
        microphoneDropdown.gameObject.SetActive(true);
    }

    private void OnMicrophoneChanged(int index)
    {
        selectedMicrophone = microphoneDropdown.options[index].text;
        Debug.Log("Selected Microphone: " + selectedMicrophone);
        PlayerPrefs.SetString("SelectedMicrophone", selectedMicrophone);
    }

    public void ShowMicrophoneDropdown()
    {
        if (microphoneDropdown != null)
            microphoneDropdown.gameObject.SetActive(true);
    }

    public void LoadSettings()
    {
        if (PlayerPrefs.HasKey("Volume") && volumeSlider != null)
        {
            float savedVolume = PlayerPrefs.GetFloat("Volume");
            volumeSlider.value = savedVolume;
            AudioListener.volume = savedVolume;
        }

        if (PlayerPrefs.HasKey("Resolution") && resolutionDropdown != null)
        {
            int savedResolution = PlayerPrefs.GetInt("Resolution");
            resolutionDropdown.value = savedResolution;
            resolutionDropdown.RefreshShownValue();

            SetResolution(savedResolution);
        }

        if (PlayerPrefs.HasKey("SelectedMicrophone") && microphoneDropdown != null)
        {
            string savedMic = PlayerPrefs.GetString("SelectedMicrophone");
            int index = microphoneDropdown.options.FindIndex(option => option.text == savedMic);
            if (index != -1)
            {
                microphoneDropdown.value = index;
                microphoneDropdown.RefreshShownValue();
                selectedMicrophone = savedMic;
            }
        }
    }

    private void AdjustCamera()
    {
        Camera cam = Camera.main;

        if (cam != null)
        {
            float targetAspect = 16.0f / 9.0f;
            float windowAspect = (float)Screen.width / (float)Screen.height;
            float scaleHeight = windowAspect / targetAspect;

            if (scaleHeight < 1.0f)
            {
                Rect rect = cam.rect;
                rect.width = 1.0f;
                rect.height = scaleHeight;
                rect.x = 0;
                rect.y = (1.0f - scaleHeight) / 2.0f;
                cam.rect = rect;
            }
            else
            {
                Rect rect = cam.rect;
                float scaleWidth = 1.0f / scaleHeight;
                rect.width = scaleWidth;
                rect.height = 1.0f;
                rect.x = (1.0f - scaleWidth) / 2.0f;
                rect.y = 0;
                cam.rect = rect;
            }
        }
    }
}
