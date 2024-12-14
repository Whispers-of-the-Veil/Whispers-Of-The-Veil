//Farzana Tanni

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorTransition : MonoBehaviour
{
    [Header("Scene Names")]
    public string cabinScene = "Cabin";
    public string demoScene = "Demo Scene";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            string currentScene = SceneManager.GetActiveScene().name;

            if (currentScene == cabinScene)
            {
                SceneManager.LoadScene(demoScene);
            }
            else if (currentScene == demoScene)
            {
                SceneManager.LoadScene(cabinScene);
            }
        }
    }
}