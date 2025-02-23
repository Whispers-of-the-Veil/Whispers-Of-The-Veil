//Sasha Koroleva
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class InventoryView : MonoBehaviour
{
    [SerializeField] private Image[] slotImages;
    
    public void UpdateSlot(int slotIndex, Sprite sprite)
    {
        if (slotIndex < 0 || slotIndex >= slotImages.Length) return;

        // Set the slot's icon
        slotImages[slotIndex].sprite = sprite;
        // Enable the image component only if there's a sprite
        slotImages[slotIndex].enabled = (sprite != null);
    }
}
