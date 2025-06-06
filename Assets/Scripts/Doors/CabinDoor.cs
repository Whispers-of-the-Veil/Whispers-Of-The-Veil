//Farzana Tanni

using UnityEngine;
using UnityEngine.SceneManagement;

public class CabinDoor : MonoBehaviour
{
    private bool enterAllowed = false;
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
            enterAllowed = true;
            Debug.Log("Near Door: Press 'G' to enter.");
            if (promptText != null)
                promptText.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            enterAllowed = false;
            Debug.Log("Left the door area.");
            if (promptText != null)
                promptText.SetActive(false);
        }
    }

    void Update()
    {
        if (enterAllowed && Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("G Key Pressed! Entering Cabin...");
            if (promptText != null)
                promptText.SetActive(false);
            SceneManager.LoadScene("CabinMain");
            SceneManager.sceneLoaded += OnSceneLoaded; 
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "CabinMain")
        {
            GameObject spawnPoint = GameObject.Find("CabinSpawn_MainDoor"); 

            if (spawnPoint != null && player != null)
            {
                player.transform.position = spawnPoint.transform.position;
                //player.transform.localScale = new Vector3(1.7f, 1.7f, 1.7f);
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