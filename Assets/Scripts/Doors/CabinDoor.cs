/*
Farzana Tanni
*/

using UnityEngine;
using UnityEngine.SceneManagement;

public class CabinDoor : MonoBehaviour
{
    private bool enterAllowed = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            enterAllowed = true;
            Debug.Log("Near Door: Press 'G' to enter.");
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
            Debug.Log("G Key Pressed! Entering Cabin...");
            SceneManager.LoadScene("Cabin");
        }
    }
}