// Farzana Tanni 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Characters.Player.Speech;

public class SwitchInputButton : MonoBehaviour
{
    public Voice playerVoice; // Drag the Player (with Voice script) here

    private Text buttonText;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(ToggleInputMethod);
        buttonText = GetComponentInChildren<Text>();
        UpdateButtonText();
    }

    void ToggleInputMethod()
    {
        if (playerVoice != null)
        {
            playerVoice.useSpeechModel = !playerVoice.useSpeechModel; 
            UpdateButtonText();
        }
    }

    void UpdateButtonText()
    {
        if (buttonText != null)
        {
            buttonText.text = playerVoice.useSpeechModel ? "Voice Input" : "Text Input"; 
        }
    }
}