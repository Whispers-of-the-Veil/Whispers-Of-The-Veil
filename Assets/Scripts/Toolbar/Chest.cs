//Sasha Koroleva
using System.Collections;
using System.Collections.Generic;
using Characters.Player;
using UnityEngine;
using Characters.Player.Speech; 

public class Chest : MonoBehaviour
{
    [SerializeField] private Sprite openChestSprite;
    [SerializeField] private GameObject antidotePrefab;
    private bool isOpen = false;

    private const string playerTag = "Player";
    public Key key
    {
        get => Key.instance;
    }


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
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                Vector3 spawnOffset = new Vector3(.1f, -0.1f, 0); 
                Vector3 spawnPosition = player.transform.position + spawnOffset;

                Instantiate(antidotePrefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                Debug.LogError("chest_sprite child not found!");
                return;
            }
            
            
            // Destroy the key after using it to open the chest.
            Destroy(Key.instance);
            isOpen = true;
            Debug.Log("Chest opened via voice command!");
        }
    }
}
