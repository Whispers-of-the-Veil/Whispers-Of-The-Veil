//Farzana Tanni

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D point)
    {
        if (point.CompareTag("Player"))
        {
            CheckpointManager.Instance.SetCheckpoint(transform.position);
            Debug.Log("Checkpoint updated: " + transform.position);
        }
    }
}