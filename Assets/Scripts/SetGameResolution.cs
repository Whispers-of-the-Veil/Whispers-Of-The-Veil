// Farzana Tanni

using UnityEngine;

public class SetGameResolution : MonoBehaviour
{
    // Reference resolution
    private const int baseWidth = 1920;
    private const int baseHeight = 1080;
    private const float targetAspect = 16f / 9f;
    private const int padding = 0; 

    void Awake()
    {
        int screenWidth = Display.main.systemWidth;
        int screenHeight = Display.main.systemHeight;

        float screenAspect = (float)screenWidth / screenHeight;

        int finalWidth = baseWidth;
        int finalHeight = baseHeight;

        if (screenWidth >= baseWidth && screenHeight >= baseHeight)
        {
            finalWidth = baseWidth;
            finalHeight = baseHeight;
        }
        else
        {
            if (screenAspect >= targetAspect)
            {
                finalHeight = screenHeight - padding;
                finalWidth = Mathf.RoundToInt(finalHeight * targetAspect);
            }
            else
            {
                finalWidth = screenWidth - padding;
                finalHeight = Mathf.RoundToInt(finalWidth / targetAspect);
            }
        }

        Screen.SetResolution(finalWidth, finalHeight, false);
        Debug.Log($"[Resolution Set] {finalWidth} x {finalHeight}, Monitor: {screenWidth} x {screenHeight}");

        DontDestroyOnLoad(gameObject);
    }
}