//Farzana Tanni

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string sceneToLoad; // Stores the scene name for trigger transitions
    private GameObject player;

    // For UI Buttons (Assign Scene Name in Inspector)
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // For Colliders (Automatically Loads When Player Enters a Trigger)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !string.IsNullOrEmpty(sceneToLoad)) // Check if scene name is set
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError("Scene name is missing! Set it in the Inspector.");
        }
    }

    // Set Scene Name for Trigger Zones (Assign in Inspector)
    public void SetSceneToLoad(string sceneName)
    {
        sceneToLoad = sceneName;
    }

    public void ExitGame()
    {
    Application.Quit();
    Debug.Log("Game is closing...");
    }
}
