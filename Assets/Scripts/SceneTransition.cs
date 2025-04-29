// Farzana Tanni

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string sceneToLoad; // stores the scene name for trigger transitions
    private GameObject player;

    // for UI Buttons (Assign Scene Name in Inspector)
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // for Colliders (Automatically Loads When Player Enters a Trigger)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !string.IsNullOrEmpty(sceneToLoad)) // check if scene name is set
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.Log("Scene name is missing! Set it in the Inspector.");
        }
    }

    // set Scene Name for Trigger Zones (Assign in Inspector)
    public void SetSceneToLoad(string sceneName)
    {
        sceneToLoad = sceneName;
    }

    // used by buttons to open Settings Popup!
    public void OpenSettingsPopup()
    {
        if (SettingsManager.instance != null)
        {
            SettingsManager.instance.OpenSettings();
        }
        else
        {
            Debug.LogWarning("SettingsManager instance not found!");
        }
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Game is closing...");
    }
}