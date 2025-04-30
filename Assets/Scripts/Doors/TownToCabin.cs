// Farzana Tanni

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TownToCabin : MonoBehaviour
{
    private bool enterAllowed = false;
    private GameObject player;
    [SerializeField] private GameObject promptText;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (promptText != null)
            promptText.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            enterAllowed = true;
            Debug.Log("Near Cabin Door: Press 'G' to enter.");
            if (promptText != null)
                promptText.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            enterAllowed = false;
            Debug.Log("Left the Cabin Door area.");
            if (promptText != null)
                promptText.SetActive(false);
        }
    }

    void Update()
    {
        if (enterAllowed && Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("G Key Pressed! Entering CabinMain...");
            if (promptText != null)
                promptText.SetActive(false);
            SceneManager.LoadScene("CabinMain");
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "CabinMain")
        {
            GameObject newSpawn = GameObject.Find("CabinSpawn_TownDoor");
            GameObject newPlayer = GameObject.FindGameObjectWithTag("Player");

            if (newSpawn != null && newPlayer != null)
            {
                newPlayer.transform.position = newSpawn.transform.position;
                Debug.Log("Player moved to CabinSpawn_TownDoor.");
            }
            else
            {
                Debug.LogWarning("CabinSpawn_TownDoor not found!");
            }
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
