// Farzana Tanni

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Characters.Player.Speech;

public class PersistentPauseMenu : MonoBehaviour
{
    public static PersistentPauseMenu instance;

    [Header("UI Elements")]
    public GameObject pauseUI;
    public Button resumeButton;
    public Button saveButton;
    public Button settingsButton;
    public Button quitButton;
    public Button toggleVoiceButton; 
    public TMP_Text toggleVoiceButtonText; 

    [Header("Player Voice Input")]
    public Voice playerVoice;

    private bool isPaused = false;

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

        if (pauseUI != null)
            pauseUI.SetActive(false);
    }

    private void Start()
    {
        resumeButton.onClick.AddListener(ResumeGame);
        saveButton.onClick.AddListener(SaveGame);
        settingsButton.onClick.AddListener(OpenSettings);
        quitButton.onClick.AddListener(QuitToMainMenu);
        toggleVoiceButton.onClick.AddListener(ToggleVoiceInput);

        if (playerVoice == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerVoice = player.GetComponent<Voice>();
        }

        UpdateToggleVoiceText();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        pauseUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        pauseUI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void SaveGame()
    {
        Debug.Log("Save button clicked!");

        if (SaveManager.instance != null)
        {
            SaveManager.instance.OpenSavePanel(); 
            pauseUI.SetActive(false);  
        }
        else
        {
            Debug.LogWarning("SaveManager instance not found!");
        }
    }

    public void OpenSettings()
    {
        Debug.Log("Opening Settings from Pause Menu...");

        if (pauseUI != null)
            pauseUI.SetActive(false);

        if (SettingsManager.instance != null)
            SettingsManager.instance.OpenSettings();
        else
            Debug.LogWarning("SettingsManager not found!");
    }

    public void CloseSavePanel()
    {
        if (SaveManager.instance != null)
            SaveManager.instance.CloseSavePanel();
            
    }

    public void CloseSettings()
    {
        if (SettingsManager.instance != null)
            SettingsManager.instance.CloseSettings();
            PauseGame();
    }

    public void QuitToMainMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;
        Debug.Log("Returning to Title Screen...");
        SceneManager.LoadScene("Main Menu");
    }

    public void ToggleVoiceInput()
    {
        if (playerVoice != null)
        {
            playerVoice.useSpeechModel = !playerVoice.useSpeechModel;
            Debug.Log("voice input " + (playerVoice.useSpeechModel ? "on" : "off"));
            UpdateToggleVoiceText();
        }
        else
        {
            Debug.LogWarning("Player voice component not found!");
        }
    }

    private void UpdateToggleVoiceText()
    {
        if (toggleVoiceButtonText != null)
        {
            toggleVoiceButtonText.text = playerVoice != null && playerVoice.useSpeechModel
                ? "Switch to Text Input"
                : "Switch to Voice Input";
        }
    }
}
