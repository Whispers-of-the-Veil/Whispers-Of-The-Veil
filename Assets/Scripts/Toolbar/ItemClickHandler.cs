//Sasha Koroleva

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemClickHandler : MonoBehaviour
{
    public Inventory _inventory;

    public void OnItemClicked()
    {
        ItemDragHandler dragHandler = gameObject.transform.Find("ItemImage").GetComponent<ItemDragHandler>();

        IInventoryItem item = dragHandler.Item;
        
        _inventory.UseItem(item);

        item.OnUse();

        Image itemImage = gameObject.transform.Find("ItemImage").GetComponent<Image>();
        itemImage.enabled = false;

    }
}