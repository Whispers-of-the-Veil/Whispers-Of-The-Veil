//Farzana Tanni

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public void PlayGame()
    {
        SceneManager.LoadSceneAsync(2); 
    }

    public void OpenSettings()
    {
        SceneManager.LoadSceneAsync("Settings");
    }

    public void OpenSaveFiles()
    {
        SceneManager.LoadSceneAsync("SaveFiles");
    }

    public void BackButton()
    {
        SceneManager.LoadSceneAsync("Main Menu");
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("exit game");
    }
}
