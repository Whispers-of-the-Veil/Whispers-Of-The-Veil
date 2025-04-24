//Farzana Tanni

using UnityEngine;
using UnityEngine.SceneManagement;

public class CabinExit : MonoBehaviour
{
    private bool exitAllowed = false;
    private GameObject player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            exitAllowed = true;
            Debug.Log("Near Exit: Press 'F' to go outside.");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            exitAllowed = false;
            Debug.Log("Left the exit zone.");
        }
    }

    void Update()
    {
        if (exitAllowed && Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("F Pressed! Leaving Cabin to Town...");
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
                player.transform.localScale = new Vector3(1.7f, 1.7f, 1.7f); // match whatever scale you use
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