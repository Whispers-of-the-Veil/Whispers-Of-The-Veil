//Farzana Tanni

using UnityEngine;

public class SaveManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject saveSlotsPanel; 
    public GameObject optionsMenu;

    private int currentSaveSlot = -1;

    private void Start()
    {
        if (saveSlotsPanel != null)
        {
            saveSlotsPanel.SetActive(false);
        }
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

        Debug.Log("save slots panel opened");
    }

    public void SaveToSlot(int slotNumber)
    {
        currentSaveSlot = slotNumber;
        Debug.Log($"game saved to slot {slotNumber}");

        PlayerPrefs.SetInt($"saveSlot{slotNumber}_Data", slotNumber);
        PlayerPrefs.Save();

        CloseSaveSlots();
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

        Debug.Log("save slots panel closed");
    }

    public void LoadFromSlot(int slotNumber)
    {
        if (PlayerPrefs.HasKey($"SaveSlot{slotNumber}_Data"))
        {
            int savedData = PlayerPrefs.GetInt($"SaveSlot{slotNumber}_Data");
            Debug.Log($"loaded data from slot {slotNumber}: {savedData}");
        }
        else
        {
            Debug.Log($"no save data found for slot {slotNumber}");
        }
    }
}

