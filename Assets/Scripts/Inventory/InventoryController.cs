//Sasha Koroleva
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [Header("MVC References")]
    [SerializeField] private InventoryView inventoryView;
    
    [Header("Inventory Settings")]
    [SerializeField] private int inventorySize = 6;

    [Header("Player / Equipment Setup")]
    [SerializeField] private Transform holdPoint; // The point on the player where items are held
    
    // The underlying Model that stores inventory data
    private InventoryModel inventoryModel;

    // Keep track of the currently equipped item (as a GameObject in the scene)
    private GameObject currentlyEquippedItem;

    private void Awake()
    {
        // Initialize the inventory model with the specified size
        inventoryModel = new InventoryModel(inventorySize);
    }

    private void Update()
    {
        // Check for 'Q' to pick up an item
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // Here, you could do a raycast or some other logic to pick up an item from the world.
            // For simplicity, let's pretend we just "spawn" a random item.
            Item newItem = CreateRandomItem();
            AttemptToPickUpItem(newItem);
        }

        // Check if any numeric key 1–6 was pressed to equip from the corresponding slot
        for (int i = 1; i <= inventorySize; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                AttemptToEquipItem(i - 1); // Convert 1-based to 0-based index
            }
        }
    }
    
    private void AttemptToPickUpItem(Item item)
    {
        // If the inventory is full, AddItem() will return false
        bool added = inventoryModel.AddItem(item);
        if (added)
        {
            // Update the entire UI to reflect the new item in a slot
            UpdateAllSlots();
            Debug.Log($"Picked up: {item.itemName}");
        }
        else
        {
            Debug.Log("Inventory is full! Cannot pick up more items.");
        }
    }
    
    private void AttemptToEquipItem(int slotIndex)
    {
        // If we're already holding an item, we can't equip another
        if (currentlyEquippedItem != null)
        {
            Debug.Log("You're already holding an item!");
            return;
        }

        Item item = inventoryModel.GetItem(slotIndex);
        if (item != null)
        {
            // Instantiate the item prefab at the 'holdPoint'
            currentlyEquippedItem = Instantiate(item.itemPrefab, holdPoint.position, holdPoint.rotation, holdPoint);

            // Remove the item from the inventory model
            inventoryModel.RemoveItem(slotIndex);

            // Update the UI to remove the icon from that slot
            inventoryView.UpdateSlot(slotIndex, null);

            Debug.Log($"Equipped: {item.itemName}");
        }
        else
        {
            Debug.Log("No item in that slot to equip!");
        }
    }

    private void UpdateAllSlots()
    {
        for (int i = 0; i < inventorySize; i++)
        {
            Item item = inventoryModel.GetItem(i);
            Sprite sprite = (item != null) ? item.itemIcon : null;
            inventoryView.UpdateSlot(i, sprite);
        }
    }

    private Item CreateRandomItem()
    {
        // In a real scenario, you might have a database of items or pick up an item from the scene.
        // This is just for demonstration.
        Item randomItem = new Item
        {
            itemName = "Item " + Random.Range(1, 1000),
            // You would assign a real sprite here
            itemIcon = null,
            // You would assign a real prefab here (drag from inspector or use a resource)
            itemPrefab = null
        };

        return randomItem;
    }
}
