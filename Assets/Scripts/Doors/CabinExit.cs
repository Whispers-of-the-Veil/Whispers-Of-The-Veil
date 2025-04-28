//Farzana Tanni

using UnityEngine;
using UnityEngine.SceneManagement;

public class CabinExit : MonoBehaviour
{
    private bool exitAllowed = false;
    private GameObject player;
    [SerializeField] private GameObject promptText;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (promptText != null)
            promptText.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            exitAllowed = true;
            Debug.Log("Near Exit: Press 'G' to go outside.");
            if (promptText != null)
                promptText.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            exitAllowed = false;
            Debug.Log("Left the exit zone.");
            if (promptText != null)
                promptText.SetActive(false);
        }
    }

    void Update()
    {
        if (exitAllowed && Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("G Pressed! Leaving Cabin to Town...");
            if (promptText != null)
                promptText.SetActive(false);
            SceneManager.LoadScene("Town_Main");
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Town_Main")
        {
            GameObject spawnPoint = GameObject.Find("CabinBackExit"); // place this GameObject in Town

            if (spawnPoint != null && player != null)
            {
                player.transform.position = spawnPoint.transform.position;
                Debug.Log("Player moved to CabinBackExit.");
            }
            else
            {
                Debug.LogWarning("CabinBackExit not found in Town!");
            }
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}