//Farzana Tanni

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class TownBellManager : MonoBehaviour
{
    public AudioClip bellSound;// assign in inspector
    private AudioSource audioSource;// will be added at runtime
    private float timer = 0f;
    private float interval = 180f;// 3 minutes = 180 seconds

    public static event Action OnBellRing;//for future monster, shake

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Town_Main")
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = bellSound;
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name != "Town_Main")
            return;

        timer += Time.unscaledDeltaTime;

        if (timer >= interval)
        {
            timer = 0f;
            PlayBell();
            OnBellRing?.Invoke(); //triggers extra effects if you want later
        }
    }

    void PlayBell()
    {
        if (audioSource != null && bellSound != null)
            audioSource.Play();
    }
}
