//Farzana Tanni 

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ClearVolumePrefOnExit
{
    static ClearVolumePrefOnExit()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                Debug.Log("Resetting volume to 0.5 on exit.");
                PlayerPrefs.SetFloat("Volume", 0.5f);
                PlayerPrefs.Save();
            }
        };
    }
}
#endif