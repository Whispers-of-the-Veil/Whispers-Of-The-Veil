//Sasha Koroleva

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class HUD : MonoBehaviour
{
    public Inventory Inventory
    {
        get => Inventory.instance;
    }

    void Start()
    {
        foreach(IInventoryItem i in Inventory.mItems)
        {
            populateNextSlot(i);
        }
        Inventory.ItemAdded += InventoryScript_ItemAdded;
    }


    private void OnDestroy()
    {
        Inventory.ItemAdded -= InventoryScript_ItemAdded;
    }

    private void populateNextSlot(IInventoryItem inventoryItem)
    {
        Transform inventoryPanel = transform.Find("Inventory");
        foreach (Transform slot in inventoryPanel)
        {
            Transform imageTransform = slot.GetChild(0).GetChild(0);
            Image image = imageTransform.GetComponent<Image>();
            ItemDragHandler itemDragHandler = imageTransform.GetComponent<ItemDragHandler>();

            if (!image.enabled)
            {
                image.enabled = true;
                image.sprite = inventoryItem.Image;

                itemDragHandler.Item = inventoryItem;

                break;
            }
        }
    }

    private void InventoryScript_ItemAdded(object sender, InventoryEventArgs e)
    {
        populateNextSlot(e.Item);
        
    }

    private void Inventory_ItemRemoved(object sender, InventoryEventArgs e)
    {
        Transform inventoryPanel = transform.Find("Inventory");
        foreach (Transform slot in inventoryPanel)
        {
            Transform imageTransform = slot.GetChild(0).GetChild(0);
            Image image = imageTransform.GetComponent<Image>();
            ItemDragHandler itemDragHandler = imageTransform.GetComponent<ItemDragHandler>();

            if (itemDragHandler.Item.Equals(e.Item))
            {
                image.enabled = false;
                image.sprite = null;
                itemDragHandler.Item = null;
                break;
            }
        }
    }
}