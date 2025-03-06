// Farzana Tanni

using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public GameObject holdPoint;

    void Awake()
    {
        Debug.Log("PlayerManager Awake called!");
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("Destroying duplicate player");
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        instance = null;
    }

    void Start()
    {
        MoveToSpawnPoint(); 
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        MoveToSpawnPoint(); 
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void MoveToSpawnPoint()
    {
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("Spawnpoint");
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.transform.position; 
            Debug.Log($"Player moved to spawn point at {spawnPoint.transform.position}");
        }
        else
        {
            Debug.LogWarning("No spawn point found in this scene!");
        }
    }
}
