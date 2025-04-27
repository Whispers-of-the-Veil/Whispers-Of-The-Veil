using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MinimapFollow : MonoBehaviour
{
    private Transform player;

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Town_Main")
        {
            // finds player after scene loads
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    void LateUpdate()
    {
        if (player != null)
        {
            // follow the player 
            Vector3 newPos = player.position;
            newPos.z = transform.position.z; // keep minimap camera height
            transform.position = newPos;
        }
    }
}