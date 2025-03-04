//Farzana Tanni

using UnityEngine;

public class InsideCabinDoor : MonoBehaviour
{
    public Transform teleportLocation; // Assign this in the Inspector
    private bool enterAllowed = false;
    private GameObject player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player"); // Find the Player
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            enterAllowed = true;
            Debug.Log("Near Room Door: Press 'G' to enter.");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            enterAllowed = false;
            Debug.Log("Left the door area.");
        }
    }

    void Update()
    {
        if (enterAllowed && Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("G Key Pressed! Teleporting...");
            player.transform.position = teleportLocation.position; // Move player to new room
        }
    }
}
