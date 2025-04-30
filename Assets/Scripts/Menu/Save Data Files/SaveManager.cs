// Farzana Tanni

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    [Header("UI Panels")]
    public GameObject savePanel;

    [Header("Slot UI References")]
    public Button[] loadButtons; 
    public Button[] saveButtons; 
    public TMP_Text[] slotTexts;

    [Header("Back Button")]
    public Button backButton; 

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        UpdateSlotDisplay();

        if (savePanel != null)
            savePanel.SetActive(false);

        if (backButton != null)
            backButton.onClick.AddListener(CloseSavePanel);
    }

    public void OpenSavePanel()
    {
        UpdateSlotDisplay();

        if (savePanel != null)
            savePanel.SetActive(true);
    }

    public void CloseSavePanel()
    {
        if (savePanel != null)
            savePanel.SetActive(false);

        PersistentPauseMenu.instance?.PauseGame();
    }

    public void SaveToSlot(int slot)
    {
        DataPersistenceManager.Instance.SaveGame(slot);
        UpdateSlotDisplay();
    }

    public void LoadFromSlot(int slot)
    {
        DataPersistenceManager.Instance.LoadGame(slot);
    }

    private void UpdateSlotDisplay()
    {
        if (DataPersistenceManager.Instance == null)
        {
            Debug.LogWarning("DataPersistenceManager not found yet. Skipping slot update.");
            return;
        }

        for (int i = 0; i < slotTexts.Length; i++)
        {
            SaveData data = DataPersistenceManager.Instance.GetSaveSlotData(i + 1);

            if (data != null && !string.IsNullOrEmpty(data.savedTime))
            {
                slotTexts[i].text = $"Saved on {data.savedTime}";
                loadButtons[i].interactable = true;
            }
            else
            {
                slotTexts[i].text = "Empty";
                loadButtons[i].interactable = false;
            }
        }
    }

    public void DeleteAllSaves()
    {
        DataPersistenceManager.Instance.DeleteAllSaves();
        UpdateSlotDisplay();
        Debug.Log("All save files cleared!");
    }
}
