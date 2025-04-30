// Farzana Tanni

using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;

public class DataPersistenceManager : MonoBehaviour
{
    public static DataPersistenceManager Instance { get; private set; }

    [Header("Player Prefab")]
    public GameObject playerPrefab;

    private string SavePath(int slot) => Path.Combine(Application.persistentDataPath, $"saveSlot{slot}.json");

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
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found for saving!");
            return;
        }

        SaveData data = new SaveData(slot, SceneManager.GetActiveScene().name, player.transform.position);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath(slot), json);
        Debug.Log($"Saved to slot {slot} at {data.savedTime}");
    }

    public void LoadGame(int slot)
    {
        string path = SavePath(slot);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"No save file in slot {slot}.");
            return;
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        SceneManager.LoadScene(data.sceneName);

        SceneManager.sceneLoaded += (scene, mode) =>
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player") ?? Instantiate(playerPrefab);
            player.name = "Player";
            player.transform.position = new Vector3(data.playerX, data.playerY, data.playerZ);
        };
    }

    public SaveData GetSaveSlotData(int slot)
    {
        string path = SavePath(slot);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<SaveData>(json);
        }
        return null;
    }

    public void DeleteAllSaves()
    {
        for (int i = 1; i <= 3; i++)
        {
            string path = SavePath(i);
            if (File.Exists(path))
                File.Delete(path);
        }
        Debug.Log("All save files deleted.");
    }
}  
