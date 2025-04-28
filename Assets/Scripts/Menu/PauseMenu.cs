//Farzana Tanni

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Characters.Player.Speech;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    private bool isPaused = false;

    // --- NEW --- //
    public Voice playerVoice; // Reference to the Voice script

    void Start()
    {
        if (pauseMenuUI == null) {
            pauseMenuUI = GameObject.Find("PauseMenu");
        }
        pauseMenuUI.SetActive(false);

        // --- NEW --- //
        if (playerVoice == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerVoice = player.GetComponent<Voice>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OpenSettings()
    {
        Debug.Log("Settings Opened!");
    }

    public void OpenSaveFileScene()
    {
        PlayerPrefs.SetString("PreviousScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();

        Debug.Log("Opening Save File Scene...");
        Time.timeScale = 1f;
        SceneManager.LoadScene("SaveFiles");
    }

    public void ExitGame()
    {
        Debug.Log("Exiting Game...");
        Application.Quit();
    }

    // --- NEW --- //
    public void ToggleVoiceInput()
    {
        if (playerVoice != null)
        {
            playerVoice.useSpeechModel = !playerVoice.useSpeechModel;
            Debug.Log("Voice input mode switched: " + (playerVoice.useSpeechModel ? "Voice Model" : "Text Input"));
        }
    }
}
