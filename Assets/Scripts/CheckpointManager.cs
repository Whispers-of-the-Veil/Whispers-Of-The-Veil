//Farzana Tanni & Owenm Ingram

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    private Vector3 lastCheckpointPosition;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetCheckpoint(Vector3 position)
    {
        lastCheckpointPosition = position;
    }

    public Vector3 GetLastCheckpointPosition()
    {
        return lastCheckpointPosition;
    }

    public void RespawnPlayer(GameObject player)
    {
        if (lastCheckpointPosition != Vector3.zero)
        {
            player.transform.position = lastCheckpointPosition;
        }
        else
        {
            player.transform.position = new Vector3(0, 0.206f, 0);
            Debug.LogWarning("No checkpoint set, respawning at default position.");
        }
    }

}
