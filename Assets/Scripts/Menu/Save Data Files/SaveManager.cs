// Farzana Tanni

using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject saveSlotsPanel;
    public GameObject optionsMenu;

    private int currentSaveSlot = -1;
    private string previousScene;

    private void Start()
    {
        if (saveSlotsPanel != null)
        {
            saveSlotsPanel.SetActive(false);
        }

         previousScene = PlayerPrefs.GetString("PreviousScene", "Main Menu");
    }

    public void ShowSaveSlots()
    {
        if (saveSlotsPanel != null)
        {
            saveSlotsPanel.SetActive(true);
        }

        if (optionsMenu != null)
        {
            optionsMenu.SetActive(false);
        }

        Debug.Log("Save slots panel opened");
    }

    public void SaveToSlot(int slotNumber)
    {
        Debug.Log($"Save Slot {slotNumber} clicked!");

        if (DataPersistenceManager.Instance == null)
        {
            Debug.LogError("DataPersistenceManager is missing! Make sure it's in the scene.");
            return;
        }

        DataPersistenceManager.Instance.SaveGame(slotNumber);
        Debug.Log($"Game saved to slot {slotNumber}!");
    }

    public void CloseSaveSlots()
    {
        if (saveSlotsPanel != null)
        {
            saveSlotsPanel.SetActive(false);
        }

        if (optionsMenu != null)
        {
            optionsMenu.SetActive(true);
        }

        Debug.Log("Save slots panel closed");
    }

    public void LoadFromSlot(int slotNumber)
    {
        Debug.Log($"Loading game from slot {slotNumber}");

        DataPersistenceManager.Instance.LoadGame(slotNumber);
    }

    public void BackToPreviousScene()
    {
        Debug.Log($"Returning to {previousScene}...");
        SceneManager.LoadScene(previousScene);
    }
}
