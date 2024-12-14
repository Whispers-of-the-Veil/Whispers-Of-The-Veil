//Sasha Koroleva

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    [SerializeField] private Sprite openChestSprite;
    [SerializeField] private GameObject antidotePrefab;
    private bool isOpen = false;

    public void Open(GameObject keyObject)
    {
        if (!isOpen)
        {
            Transform chestSpriteTransform = transform.Find("Chest_Sprite");
            if (chestSpriteTransform != null)
            {
                SpriteRenderer spriteRenderer = chestSpriteTransform.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = openChestSprite;
                    Debug.Log("Chest sprite changed to open chest sprite.");
                }
                else
                {
                    Debug.LogError("No SpriteRenderer found on Chest_Sprite!");
                    return;
                }
            }
            else
            {
                Debug.LogError("Chest_Sprite child not found!");
                return;
            }

            Vector3 spawnPosition = transform.position + Vector3.up * 0.5f;
            Instantiate(antidotePrefab, spawnPosition, Quaternion.identity);
            
            if (keyObject != null)
            {
                Destroy(keyObject);
                Debug.Log("Key destroyed after opening the chest.");
            }
            
            isOpen = true;
            Debug.Log("Chest opened and antidote spawned!");
            
            
        }
        else
        {
            Debug.Log("Chest is already open.");
        }
    }
    
}
