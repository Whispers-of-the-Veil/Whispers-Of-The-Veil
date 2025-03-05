//Sasha Koroleva
using System.Collections;
using System.Collections.Generic;
using Characters.Player;
using UnityEngine;
using Characters.Player.Voice; // To access RecordAudio event

public class Chest : MonoBehaviour
{
    [SerializeField] private Sprite openChestSprite;
    [SerializeField] private GameObject antidotePrefab;
    // Reference to the key object the player must be holding in order to open the chest.
    [SerializeField] private GameObject keyObject;
    private bool isOpen = false;

    private const string playerTag = "Player";
    void OnEnable()
    {
        Voice.OnCommandRecognized += OnVoiceCommand;
    }

    void OnDisable()
    {
        Voice.OnCommandRecognized -= OnVoiceCommand;
    }

    // Callback triggered when a command is recognized.
    private void OnVoiceCommand(string command)
    {
        if (command.ToLower().Trim() == "open chest")
        {
            // Attempt to find the player.
            GameObject player = GameObject.FindWithTag(playerTag);
            if (player != null)
            {
                // Access the player's controller to check if they have the key.
                PlayerController controller = player.GetComponent<PlayerController>();
                if (controller != null)
                {
                    if (controller.hasKey)
                    {
                        // Open the chest and consume the key.
                        Open();
                        controller.hasKey = false;
                    }
                    else
                    {
                        Debug.Log("Cannot open chest: You are not holding the key!");
                    }
                }
                else
                {
                    Debug.LogError("PlayerController component not found on the player!");
                }
            }
            else
            {
                Debug.LogError("Player not found in the scene. Make sure the player has the tag 'Player'.");
            }
        }
    }


    public void Open()
    {
        if (!isOpen)
        {
            Transform chestSpriteTransform = transform.Find("chest_sprite");
            if (chestSpriteTransform != null)
            {
                SpriteRenderer spriteRenderer = chestSpriteTransform.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = openChestSprite;
                }
                else
                {
                    Debug.LogError("No SpriteRenderer found on chest_sprite!");
                    return;
                }
            }
            else
            {
                Debug.LogError("chest_sprite child not found!");
                return;
            }

            Vector3 spawnPosition = transform.position + Vector3.up * 0.5f;
            Instantiate(antidotePrefab, spawnPosition, Quaternion.identity);
            
            // Destroy the key after using it to open the chest.
            if (keyObject != null)
            {
                Destroy(keyObject);
            }
            isOpen = true;
            Debug.Log("Chest opened via voice command!");
        }
    }
}
