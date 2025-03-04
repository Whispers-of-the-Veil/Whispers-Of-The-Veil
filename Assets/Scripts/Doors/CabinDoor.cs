using UnityEngine;
using UnityEngine.SceneManagement;

public class CabinDoor : MonoBehaviour
{
    private bool enterAllowed = false;
    private GameObject player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            enterAllowed = true;
            Debug.Log("Near Door: Press 'G' to enter.");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            enterAllowed = false;
            Debug.Log("Left the door area.");
        }
    }

    void Update()
    {
        if (enterAllowed && Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("G Key Pressed! Entering Cabin...");
            SceneManager.LoadScene("CabinMain");
            SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to scene load event
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "CabinMain")
        {
            GameObject spawnPoint = GameObject.Find("CabinSpawn_MainDoor"); // Find the spawn point

            if (spawnPoint != null && player != null)
            {
                player.transform.position = spawnPoint.transform.position;
                player.transform.localScale = new Vector3(1.7f, 1.7f, 1.7f);
                Debug.Log("Player moved to CabinSpawn_MainDoor.");
            }
            else
            {
                Debug.LogWarning("CabinSpawn_MainDoor not found!");
            }
        }

        SceneManager.sceneLoaded -= OnSceneLoaded; // Unsubscribe from event
    }
}