// Sasha Koroleva
using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    // Singleton instance for easy access
    public static ItemDatabase Instance;

    // A list of items to set up in the Inspector.
    public List<ItemData> items;

    // Dictionary mapping item IDs to their prefab GameObjects.
    private Dictionary<string, GameObject> itemDictionary;

    void Awake()
    {
        // Setup the singleton instance.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            PopulateDictionary();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Populate the dictionary from the list.
    private void PopulateDictionary()
    {
        itemDictionary = new Dictionary<string, GameObject>();
        foreach (var data in items)
        {
            if (!itemDictionary.ContainsKey(data.itemID))
            {
                itemDictionary.Add(data.itemID, data.itemPrefab);
            }
            else
            {
                Debug.LogWarning("Duplicate itemID found in ItemDatabase: " + data.itemID);
            }
        }
    }

    /// <summary>
    /// Retrieves an instance of an inventory item by its unique ID.
    /// This method instantiates the prefab and returns the IInventoryItem component.
    /// </summary>
    /// <param name="id">Unique identifier for the item.</param>
    /// <returns>An IInventoryItem instance, or null if not found.</returns>
    public static IInventoryItem GetItemByID(string id)
    {
        if (Instance == null)
        {
            Debug.LogError("ItemDatabase instance not found!");
            return null;
        }

        if (Instance.itemDictionary.ContainsKey(id))
        {
            GameObject prefab = Instance.itemDictionary[id];
            // Instantiate the prefab and get the IInventoryItem component.
            GameObject instance = Instantiate(prefab);
            IInventoryItem inventoryItem = instance.GetComponent<IInventoryItem>();
            if (inventoryItem == null)
            {
                Debug.LogError("The prefab for item ID " + id + " does not have a component implementing IInventoryItem.");
            }
            return inventoryItem;
        }
        else
        {
            Debug.LogWarning("No item found with ID: " + id);
            return null;
        }
    }
}

/// <summary>
/// A serializable class to store item data in the database.
/// </summary>
[System.Serializable]
public class ItemData
{
    // Unique identifier for this item.
    public string itemID;

    // The prefab for the item. This prefab should have a component that implements IInventoryItem.
    public GameObject itemPrefab;
}

