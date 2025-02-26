// Sasha Koroleva
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class Item
{
    public string itemName;
    public Sprite itemIcon;      
    public GameObject itemPrefab; 
}

public class InventoryModel
{
    private Item[] slots;

    public InventoryModel(int size)
    {
        slots = new Item[size];
    }

    public bool AddItem(Item item)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = item;
                return true;
            }
        }
        return false;
    }
    
    public Item GetItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length)
            return null;

        return slots[slotIndex];
    }

    public void RemoveItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length)
            return;

        slots[slotIndex] = null;
    }
}
