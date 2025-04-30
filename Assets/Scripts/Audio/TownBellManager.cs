//Farzana Tanni

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Management;

public class TownBellManager : MonoBehaviour {
    public static TownBellManager instance;
    
    public AudioClip bellSound;// assign in inspector
    private AudioSource audioSource;// will be added at runtime
    private float timer = 0f;
    private float interval = 180f;// 3 minutes = 180 seconds
    
    public DontDestroyManager dontDestroyManager {
        get => DontDestroyManager.instance;
    }

    public static event Action OnBellRing;//for future monster, shake

    private void Awake() {
        if (instance == null) {
            instance = this;
            dontDestroyManager.Track(this.gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }

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
    
    private void OnDestroy() {
        if (instance == this) instance = null;
    }
}
