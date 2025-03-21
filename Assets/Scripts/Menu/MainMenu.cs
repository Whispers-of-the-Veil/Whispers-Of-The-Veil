//Farzana Tanni

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private FadeController sceneController;

    public void PlayGame()
    {
        sceneController.LoadScene("Grassy Plans"); 
    }

    public void OpenSettings()
    {
        sceneController.LoadScene("Settings");
    }

    public void OpenSaveFiles()
    {
        sceneController.LoadScene("SaveFiles");
    }

    public void BackButton()
    {
        sceneController.LoadScene("Main Menu");
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("exit game");
    }
}
