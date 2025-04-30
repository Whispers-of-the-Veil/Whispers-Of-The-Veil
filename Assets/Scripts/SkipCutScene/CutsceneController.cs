//Sasha Koroleva
using System.Collections;
using System.Collections.Generic;
using Management;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class CutsceneController : MonoBehaviour
{
    [Tooltip("Drag your Timeline's PlayableDirector here")]
    public PlayableDirector director;

    [Tooltip("Name (or build index) of the gameplay scene to load")]
    public string gameplaySceneName;
    
    public DontDestroyManager dontDestroyManager {
        get => DontDestroyManager.instance;
    }

    void Start()
    {
        // When the Timeline finishes playing, automatically transition
        director.stopped += OnDirectorStopped;
    }

    void Update()
    {
        // Example: press Space to skip
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SkipCutscene();
        }
    }

    void OnDirectorStopped(PlayableDirector pd)
    {
        // Timeline ended normally
        LoadGameplay();
    }

    public void SkipCutscene()
    {
        // Unsubscribe so we don’t double-load
        director.stopped -= OnDirectorStopped;
        director.Stop();            // halt the cutscene
        LoadGameplay();
    }

    void LoadGameplay()
    {
        if (gameplaySceneName == "EndTitleScene") dontDestroyManager.ResetAll();
        SceneManager.LoadScene(gameplaySceneName);
    }
}
