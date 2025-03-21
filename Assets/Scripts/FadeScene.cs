//Farzana Tanni

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeScene : MonoBehaviour 
{
    private Image sceneFadeImage;

    private void Awake()
    {
        sceneFadeImage = GetComponent<Image>();
        if (sceneFadeImage == null)
        {
            Debug.LogError("No Image component found on FadeScene! Assign it in the Inspector.");
            return;
        }
        sceneFadeImage.color = new Color(0, 0, 0, 1);
    }

    private void Start()
    {
    StartCoroutine(FadeInCoroutine(1f));
    }

    public IEnumerator  FadeInCoroutine(float duration)
    {
        Color startColor = new Color(sceneFadeImage.color.r, sceneFadeImage.color.g, sceneFadeImage.color.b, 1);
        Color targetColor = new Color (sceneFadeImage.color.r, sceneFadeImage.color.g, sceneFadeImage.color.b, 0);

        yield return FadeCoroutine(startColor, targetColor, duration);

        sceneFadeImage.color = new Color(0, 0, 0, 0);
    }

    public IEnumerator FadeOutCoroutine(float duration, string sceneName)
    {
        Color startColor = new Color(sceneFadeImage.color.r, sceneFadeImage.color.g, sceneFadeImage.color.b, 1);
        Color targetColor = new Color (sceneFadeImage.color.r, sceneFadeImage.color.g, sceneFadeImage.color.b, 0);

        yield return FadeCoroutine(startColor, targetColor, duration);

        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeCoroutine(Color startColor, Color targetColor, float duration)
    {
        float elapsedTime = 0;
        float elapsedPercentage = 0;

        while(elapsedPercentage < 1)
        {
            elapsedPercentage = elapsedTime / duration;
            sceneFadeImage.color = Color.Lerp(startColor, targetColor, elapsedPercentage);

            yield return null;
            elapsedTime += Time.deltaTime;
        }

        sceneFadeImage.color = targetColor;
    }
}