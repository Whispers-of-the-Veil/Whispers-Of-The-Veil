//Farzana Tanni

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOptionsManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject optionsMenu;
    public GameObject settingsMenu;
    public Button saveButton; 
    public Button settingsButton;
    public Button mainMenuButton;
    public Button exitButton;
    public Text feedbackText;

    private bool isGamePaused = false;

    private void Start()
    {
        if (optionsMenu != null)
        {
            optionsMenu.SetActive(false);
        }

        if (saveButton != null)
        {
            saveButton.onClick.AddListener(SaveGame);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OpenSettings);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitGame);
        }
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleOptionsMenu();
        }
    }

    private void ToggleOptionsMenu()
    {
        isGamePaused = !isGamePaused;
        optionsMenu.SetActive(isGamePaused);

        Time.timeScale = isGamePaused ? 0 : 1;

        if (feedbackText != null)
        {
            feedbackText.text = isGamePaused ? "game paused" : "game resumed";
        }

        Debug.Log(isGamePaused ? "game paused" : "game resumed");
    }

    public void SaveGame()
    {

        Debug.Log("game saved!");

        if (feedbackText != null)
        {
            feedbackText.text = "game saved successfully!";
        }
    }

    public void OpenSettings()
    {
        optionsMenu.SetActive(false);
        settingsMenu.SetActive(true);

        if (feedbackText != null)
        {
            feedbackText.text = "opened settings menu.";
        }

        Debug.Log("settings menu opened");
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1;

        if (feedbackText != null)
        {
            feedbackText.text = "returning to Main Menu";
        }

        Debug.Log("returning to Main Menu");
        SceneManager.LoadScene("MainMenu"); 
    }

    public void ExitGame()
    {
        Debug.Log("exiting game");
        if (feedbackText != null)
        {
            feedbackText.text = "exiting game";

            Invoke(nameof(QuitGame), 1.5f); 
        }
        else
        {
            QuitGame();
        }
    }

    private void QuitGame()
    {
        Application.Quit();
    }

    public void ResumeGame()
    {
        optionsMenu.SetActive(false);
        Time.timeScale = 1;

        if (feedbackText != null)
        {
            feedbackText.text = "game resumed";
        }

        Debug.Log("game resumed");
    }
}