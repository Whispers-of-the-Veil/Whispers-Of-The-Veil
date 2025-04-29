using UnityEngine;
using Characters.Player;
using Characters.Player.Speech;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Chest : MonoBehaviour
{
    [Header("Persistence")]
    [Tooltip("Auto-generated ID for this chest")]
    [SerializeField] private string chestID;

    [Header("Contents")]
    [SerializeField] private Sprite openChestSprite;
    [SerializeField] private GameObject antidotePrefab;

    private bool isOpen = false;
    private const string playerTag = "Player";

    public Key key => Key.instance;

    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(chestID))
        {
            chestID = System.Guid.NewGuid().ToString();
            EditorUtility.SetDirty(this);
        }
    }
    #endif

    void Start()
    {
#if UNITY_EDITOR
        PlayerPrefs.DeleteKey("chest_open_" + chestID);
#endif

        if (PlayerPrefs.GetInt("chest_open_" + chestID, 0) == 1)
            ForceOpen();
    }

    private void OnEnable()
    {
        Voice.OnCommandRecognized += OnVoiceCommand;
    }

    private void OnDisable()
    {
        Voice.OnCommandRecognized -= OnVoiceCommand;
    }

    private void OnVoiceCommand(string command)
    {
        if (command.ToLower().Trim() != "open chest") return;

        var playerGO = GameObject.FindWithTag(playerTag);
        if (playerGO == null)
        {
            Debug.LogError("Player not found in scene!");
            return;
        }

        var controller = playerGO.GetComponent<PlayerController>();
        if (controller == null)
        {
            Debug.LogError("PlayerController missing on Player!");
            return;
        }

        if (!controller.hasKey)
        {
            Debug.Log("Cannot open chest: You are not holding the key!");
            return;
        }

        controller.hasKey = false;

        Open(controller.transform);
    }
    public void Open(Transform playerT = null)
    {
        if (isOpen) return;

        if (antidotePrefab != null)
        {
            Vector3 spawnPos = playerT.position + new Vector3(0.1f, -0.1f, 0);
            Instantiate(antidotePrefab, spawnPos, Quaternion.identity);
        }

        PlayerPrefs.SetInt("chest_open_" + chestID, 1);
        PlayerPrefs.Save();
        ForceOpen();

        if (Key.instance != null)
            Destroy(Key.instance);

        Debug.Log("Chest opened via voice command!");
    }

    private void ForceOpen()
    {
        var spriteTransform = transform.Find("chest_sprite");
        if (spriteTransform != null)
        {
            var sr = spriteTransform.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.sprite = openChestSprite;
            else
                Debug.LogError("No SpriteRenderer on chest_sprite!");
        }
        else
        {
            Debug.LogError("child 'chest_sprite' not found!");
        }

        isOpen = true;
    }
}
