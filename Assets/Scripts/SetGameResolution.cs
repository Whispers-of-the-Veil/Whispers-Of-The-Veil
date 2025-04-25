//Farzana Tanni

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetGameResolution : MonoBehaviour
{
    void Awake()
    {
        int screenWidth = Display.main.systemWidth;
        int screenHeight = Display.main.systemHeight;

        int padding = 40; // pixels to shrink so it fits nicely and avoids taskbars

        if (screenWidth >= 1920 && screenHeight >= 1080)
        {
            // big monitors
            Screen.SetResolution(1920 - padding, 1080 - padding, false);
        }
        else if (screenWidth >= 1366 && screenHeight >= 768)
        {
            // medium 
            Screen.SetResolution(1366 - padding, 768 - padding, false);
        }
        else
        {
            // smaller
            Screen.SetResolution(1280 - padding, 720 - padding, false);
        }

        DontDestroyOnLoad(gameObject); //stays alive between scenes
    }
}
