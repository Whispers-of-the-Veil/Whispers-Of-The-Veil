using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections;

public class DataPersistenceManager : MonoBehaviour
{
    public static DataPersistenceManager Instance { get; private set; }
    
    [Header("Player Prefab")]
    public GameObject playerPrefab; // Now visible in the Inspector!

    private string savePath => Application.persistentDataPath + "/saveSlot";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame(int slot)
    {
        string filePath = $"{Application.persistentDataPath}/saveSlot{slot}.json";

        Debug.Log($"Trying to save to: {filePath}");

        SaveData data = new SaveData
        {
            sceneName = SceneManager.GetActiveScene().name
        };

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player object not found! Make sure it's tagged as 'Player'.");
            return;
        }

        data.playerX = player.transform.position.x;
        data.playerY = player.transform.position.y;
        data.playerZ = player.transform.position.z;

        string json = JsonUtility.ToJson(data, true);

        try
        {
            File.WriteAllText(filePath, json);
            Debug.Log($"Game saved successfully to: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
        }
    }

    public void LoadGame(int slot)
    {
        string filePath = $"{savePath}{slot}.json";

        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"No save file found in slot {slot}!");
            return;
        }

        string json = File.ReadAllText(filePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        Debug.Log($"Loading scene: {data.sceneName}");
        SceneManager.LoadScene(data.sceneName);

        SceneManager.sceneLoaded += (scene, mode) =>
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player == null && playerPrefab != null)
            {
                Debug.LogWarning("Player was missing! Instantiating new Player...");
                player = Instantiate(playerPrefab);
                player.name = "Player"; 
            }

            if (player != null)
            {
                player.transform.position = new Vector3(data.playerX, data.playerY, data.playerZ);
            }
            Debug.Log($"Player position loaded: {data.playerX}, {data.playerY}, {data.playerZ}");
        };
    }
}