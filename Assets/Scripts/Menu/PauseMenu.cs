//Farzana Tanni

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    private bool isPaused = false;

    void Start()
    {
        pauseMenuUI.SetActive(false);
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
        // Store the scene the player is in before switching to settings
        Debug.Log("Settings Opened!");
    }

    public void OpenSaveFileScene()
    {
        // Store the current scene before switching to Save File
        PlayerPrefs.SetString("PreviousScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();

        Debug.Log("Opening Save File Scene...");
        Time.timeScale = 1f; // Ensure normal time flow in Save File Scene
        SceneManager.LoadScene("SaveFiles"); // Replace with your actual Save File scene name
    }


    public void ExitGame()
    {
        Debug.Log("Exiting Game...");
        Application.Quit();
    }
}
