//Sasha Koroleva

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Characters.Player;


public class ItemClickHandler : MonoBehaviour
{
    public Inventory _inventory
    {
        get { return Inventory.instance; }
    }

    public PlayerManager _playerManager
    {
        get { return PlayerManager.instance; }
    }
    public void OnItemClicked()
    {

        PlayerController playerController = FindObjectOfType<PlayerController>();
        if(playerController == null)
        {
            Debug.LogWarning("PlayerController not found.");
            return;
        }
        
        if(playerController.GetHeldObject() != null)
        {
            Debug.Log("Cannot pick up item from inventory; already holding an item.");
            return;
        }

        ItemDragHandler dragHandler = transform.Find("ItemImage").GetComponent<ItemDragHandler>();
        IInventoryItem item = dragHandler.Item;
        
        if(item == null)
        {
            Debug.LogWarning("No item found in drag handler.");
            return;
        }
        
        _inventory.UseItem(item);
        item.OnUse();
        _inventory.RemoveItem(item);

        GameObject itemObj = (item as MonoBehaviour).gameObject;
        itemObj.SetActive(true);
        itemObj.transform.parent = _playerManager.holdPoint.transform;
        itemObj.transform.position = _playerManager.holdPoint.transform.position;
        itemObj.transform.rotation = _playerManager.holdPoint.transform.rotation;

        playerController.SetHeldObject(itemObj);

        Image itemImage = transform.Find("ItemImage").GetComponent<Image>();
        itemImage.enabled = false;
    }
    
}