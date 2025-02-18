using System;
using System.Collections;
using UnityEngine;
using Audio;

namespace Environment {
    public class RainController : MonoBehaviour {
        [Header("Audio")]
        [SerializeField] AudioClip rainSound;
        [Range(0, 1)] [SerializeField] private float rainVolume;
        [SerializeField] AudioClip thunderSound;
        [Range(0, 1)] [SerializeField] private float thunderVolume;
        [SerializeField] float fadeDuration;
        private AudioSource rainAudio;
        private AudioSource thunderAudio; 
        private bool canPlay = true;

        [Header("Frequency")] 
        [SerializeField] private bool autoControl = true;
        [SerializeField] bool isRaining;
        [Range(0, 100)] [SerializeField] float rainFrequency;
        [Range(0, 100)] [SerializeField] float thunderFrequency;
        [SerializeField] float duration = 5;
        private bool running;
        
        [Header("Intensity")]
        private ParticleSystem rain;
        private ParticleSystem fog;

        void Awake() {
            try {
                // Find the AudioSource components
                rainAudio = GameObject.Find("RainSound").GetComponent<AudioSource>();
                thunderAudio = GameObject.Find("ThunderSound").GetComponent<AudioSource>();
                
                // Find the Particle system components
                rain = GameObject.Find("Rain").GetComponent<ParticleSystem>();
                fog = GameObject.Find("Fog").GetComponent<ParticleSystem>();
            } catch (NullReferenceException) {
                string error = "RainController: This GameObject requires two AudioSource components and two particle " +
                               "systems attached as children; One or more is currently missing. " +
                               "They need to have the following names: RainSound, ThunderSound, Rain, Fog";
                
                Debug.LogError(error);
                canPlay = false;
            }
        }
        
        void Start() {
            try {
                SetClips();

                // Set the volume of the audio source
                thunderAudio.volume = thunderVolume;
            } catch (UnassignedReferenceException) {
                Debug.LogError("RainController: One or both of the audio clips are missing");
                canPlay = false;
            }
        }
        
        void Update() {
            // If we set the automatic control, and it is currently running
            if (autoControl && !running) {
                StartCoroutine(StartRain());
            }
            
            if (!rainAudio.isPlaying && canPlay && isRaining) {
                StartCoroutine(FadeAudioSource.StartFade(rainAudio, fadeDuration, rainVolume));
                rainAudio.Play();
            }
            
            if (!thunderAudio.isPlaying && canPlay && isRaining) {
                if (UnityEngine.Random.Range(0, 100) < thunderFrequency) {
                    thunderAudio.Play();
                }
            }

        }

        private void FixedUpdate() {
            // Stop the particle systems if the weather effect isnt raining
            if (!isRaining) {
                rain.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                fog.Stop(false, ParticleSystemStopBehavior.StopEmitting);

                if (rainAudio.isPlaying) {
                    rainAudio.Stop();
                }

                if (thunderAudio.isPlaying) {
                    thunderAudio.Stop();
                }
            } else {
                rain.Play();
                fog.Play();
            }
        }

        void SetClips() {
            if (rainSound == null || thunderSound == null) {
                throw new UnassignedReferenceException();
            }
            
            // Set the clip the the Audio Source
            rainAudio.clip = rainSound;
            thunderAudio.clip = thunderSound;
        }

        IEnumerator StartRain() {
            running = true;
            
            if (UnityEngine.Random.Range(0, 100) < rainFrequency) {
                isRaining = true;
                Debug.Log("RainController: Starting rain");
            } else {
                isRaining = false;
                Debug.Log("RainController: Stopped rain");
            }
            
            yield return new WaitForSeconds(duration * 60);
            
            running = false;
        }
    }
}