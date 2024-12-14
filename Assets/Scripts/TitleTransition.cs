//Farzana Tanni

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleTransition : MonoBehaviour
{
    public string nextSceneName = "Main Menu";

    void Start()
    {
        Invoke("LoadNextScene", 3f);
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
