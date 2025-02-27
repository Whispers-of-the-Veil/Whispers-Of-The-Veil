//Sasha Koroleva

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    [SerializeField] private Sprite openChestSprite;
    [SerializeField] private GameObject antidotePrefab;
    private bool isOpen = false;

    public void Open(GameObject keyObject) {
        if (!isOpen) {
            Transform chestSpriteTransform = transform.Find("chest_sprite");
            if (chestSpriteTransform != null) {
                SpriteRenderer spriteRenderer = chestSpriteTransform.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null) {
                    spriteRenderer.sprite = openChestSprite;
                }

            }

            Vector3 spawnPosition = transform.position + Vector3.up * 0.5f;
            Instantiate(antidotePrefab, spawnPosition, Quaternion.identity);
            
            if (keyObject != null) {
                Destroy(keyObject);
            }
            isOpen = true;
        }
    }
}
