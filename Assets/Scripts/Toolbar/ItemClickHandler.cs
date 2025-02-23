//Sasha Koroleva

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemClickHandler : MonoBehaviour
{
    public Inventory _inventory;
    public Transform holdPoint;
    public void OnItemClicked()
    {

        ItemDragHandler dragHandler = gameObject.transform.Find("ItemImage").GetComponent<ItemDragHandler>();
        IInventoryItem item = dragHandler.Item;

        if (item == null)
        {
            Debug.LogWarning("Item is null. Make sure dragHandler.Item is assigned!");
            return;
        }
        
        _inventory.UseItem(item);
        item.OnUse();
        
        _inventory.RemoveItem(item);
        
        Image itemImage = gameObject.transform.Find("ItemImage").GetComponent<Image>();
        itemImage.enabled = false;
        Debug.Log("Item image disabled");
    }
}