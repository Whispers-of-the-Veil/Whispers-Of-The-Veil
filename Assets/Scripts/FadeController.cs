using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeController : MonoBehaviour
{
    [SerializeField] private float sceneFadeDuration;
    private FadeScene sceneFade;

    private IEnumerator Start()
    {
        //yield return new WaitForSeconds(0.1f);

        sceneFade = FindObjectOfType<FadeScene>();

        if (sceneFade == null)
        {
            Debug.LogError("No FadeScene found! Make sure the prefab exists in the scene.");
        }
        else
        {
            Debug.Log("FadeScene successfully assigned.");
            yield return sceneFade.FadeInCoroutine(sceneFadeDuration);
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        sceneFade = FindObjectOfType<FadeScene>();

        if (sceneFade == null)
        {
            Debug.LogError("No FadeScene found before fading out! Ensure the prefab exists in the scene.");
            yield break;
        }

        yield return sceneFade.FadeOutCoroutine(sceneFadeDuration, sceneName);
        yield return SceneManager.LoadSceneAsync(sceneName);
        sceneFade = FindObjectOfType<FadeScene>(); 

        if (sceneFade == null)
        {
            Debug.LogError("No FadeScene found in the new scene!");
            yield break;
        }

        yield return sceneFade.FadeInCoroutine(sceneFadeDuration);
    }
}
