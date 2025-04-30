//Sasha Koroleva
using UnityEngine;
using Characters.Player;
using Characters.Player.Speech;

public class Chest : MonoBehaviour
{
    [Header("Persistence")]
    [Tooltip("Manually assign a unique ID for this chest (e.g. \"Chest_01\").")]
    [SerializeField] private string chestID;

    [Header("Contents")]
    [SerializeField] private Sprite     openChestSprite;
    [SerializeField] private GameObject antidotePrefab;

    private bool isOpen = false;
    private const string playerTag = "Player";

    public Key key => Key.instance;

    private void Start()
    {
        if (PlayerPrefs.GetInt("chest_open_" + chestID, 0) == 1)
            ForceOpen();
    }

    private void OnEnable()  => Voice.OnCommandRecognized += OnVoiceCommand;
    private void OnDisable() => Voice.OnCommandRecognized -= OnVoiceCommand;

    private void OnVoiceCommand(string command)
    {
        if (command.Trim().ToLower() != "open chest") return;

        var playerGO = GameObject.FindWithTag(playerTag);
        if (playerGO == null) return;

        var controller = playerGO.GetComponent<PlayerController>();
        if (controller == null) return;

        if (!controller.hasKey) return;

        Open(playerGO.transform);
    }

    public void Open(Transform playerT)
    {
        if (isOpen) return;

        if (antidotePrefab != null && playerT != null)
        {
            Vector3 spawnPos = playerT.position + new Vector3(0.1f, -0.1f, 0);
            Instantiate(antidotePrefab, spawnPos, Quaternion.identity);
        }

        PlayerPrefs.SetInt("chest_open_" + chestID, 1);
        PlayerPrefs.Save();

        ForceOpen();

    }

    private void ForceOpen()
    {
        var spriteTransform = transform.Find("chest_sprite");
        if (spriteTransform != null)
        {
            var sr = spriteTransform.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.sprite = openChestSprite;
        }
        isOpen = true;
    }
}
