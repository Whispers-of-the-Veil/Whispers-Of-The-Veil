//Sasha Koroleva

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Management;

public class Inventory : MonoBehaviour
{
    private const int SLOTS = 6;

    public List<IInventoryItem> mItems = new List<IInventoryItem>();
    
    public static Inventory instance;
    public static event EventHandler<InventoryEventArgs> ItemAdded;
    public event EventHandler<InventoryEventArgs> ItemRemoved;
    public event EventHandler<InventoryEventArgs> ItemUsed;
    
    public DontDestroyManager dontDestroyManager {
        get => DontDestroyManager.instance;
    }

    protected void Awake()
    {
        if (instance == null) {
            instance = this;
            dontDestroyManager.Track(this.gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public void AddItem(IInventoryItem item)
    {
        if(item == null)
        {
            throw new System.ArgumentNullException(nameof(item), "Item cannot be null.");
        }
        
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
