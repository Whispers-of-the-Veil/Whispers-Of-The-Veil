using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform player;
    public float smoothSpeed = 5f; // Adjust for a smoother follow effect

    void Start()
    {
        FindPlayer(); // Try to find player when the scene starts
    }

    void LateUpdate() // Use LateUpdate so it doesn't interfere with movement
    {
        if (player == null)
        {
            FindPlayer(); // Keep checking until player spawns
        }
        else
        {
            FollowPlayer();
        }
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log("Camera found the Player!");
        }
    }

    void FollowPlayer()
    {
        Vector3 targetPosition = new Vector3(player.position.x, player.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}
