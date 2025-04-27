// Farzana Tanni

using UnityEngine;
using UnityEngine.SceneManagement;

public class OrphanageDoor : MonoBehaviour
{
    private bool enterAllowed = false;
    private GameObject player;
    [SerializeField] private GameObject promptText;
    [SerializeField] private string sceneToLoad;
    [SerializeField] private string destinationSpawnName;

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
            Debug.Log($"Near Orphanage Door to {sceneToLoad}: Press 'G' to enter.");
            if (promptText != null)
                promptText.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            enterAllowed = false;
            Debug.Log("Left the Orphanage Door area.");
            if (promptText != null)
                promptText.SetActive(false);
        }
    }

    void Update()
    {
        if (enterAllowed && Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log($"G Key Pressed! Entering {sceneToLoad}...");
            if (promptText != null)
                promptText.SetActive(false);
            SceneManager.LoadScene(sceneToLoad);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == sceneToLoad)
        {
            GameObject spawnPoint = GameObject.Find(destinationSpawnName);

            if (spawnPoint != null && player != null)
            {
                player.transform.position = spawnPoint.transform.position;
                Debug.Log($"Player moved to {destinationSpawnName}.");
            }
            else
            {
                Debug.LogWarning($"{destinationSpawnName} not found!");
            }
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}