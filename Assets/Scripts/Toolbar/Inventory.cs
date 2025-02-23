//Sasha Koroleva

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Inventory : MonoBehaviour
{
    private const int SLOTS = 6;

    private List<IInventoryItem> mItems = new List<IInventoryItem>();
    
    public static event EventHandler<InventoryEventArgs> ItemAdded;
    public event EventHandler<InventoryEventArgs> ItemRemoved;
    public event EventHandler<InventoryEventArgs> ItemUsed;

    public void AddItem(IInventoryItem item)
    {
        if (mItems.Count < SLOTS)
        {
            Collider2D collider = (item as MonoBehaviour).GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
                mItems.Add(item);
                item.OnPickup();

                ItemAdded?.Invoke(this, new InventoryEventArgs(item));
            }
            else
            {
                Debug.LogWarning("Collider2D is missing or already disabled for " + ((MonoBehaviour)item).name);
            }
        }
    }

    public void RemoveItem(IInventoryItem item)
    {
        if (mItems.Contains(item))
        {
            mItems.Remove(item);
            item.OnDrop();
            
            Collider2D collider = (item as MonoBehaviour).GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = true;
            }

            if (ItemRemoved != null)
            {
                ItemRemoved(this, new InventoryEventArgs(item));
            }
        }
    }

    public void UseItem(IInventoryItem item)
    {
        if (ItemUsed != null)
        {
            ItemUsed(this, new InventoryEventArgs(item));
        }
    }
}
