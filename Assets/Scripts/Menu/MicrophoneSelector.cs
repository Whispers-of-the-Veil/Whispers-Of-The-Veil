using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class MicrophoneSelector : MonoBehaviour
{
    public TMP_Dropdown microphoneDropdown;

    void Start()
    {
        PopulateDropdown();
    }

    void PopulateDropdown()
    {
        microphoneDropdown.ClearOptions();

        string[] devices = Microphone.devices;

        if (devices.Length > 0)
        {
            microphoneDropdown.AddOptions(new System.Collections.Generic.List<string>(devices));
        }
        else
        {
            microphoneDropdown.AddOptions(new System.Collections.Generic.List<string> { "No Microphone Found" });
            microphoneDropdown.interactable = false;
        }
    }
}
