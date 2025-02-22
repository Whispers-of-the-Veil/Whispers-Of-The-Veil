using System;
using System.Collections;
using UnityEngine;
using Audio;
using System.Runtime.Serialization;
using Environment.Hazards;

namespace Environment {
    public class RainController : MonoBehaviour {
        [Header("Audio")]
        [SerializeField] AudioClip[] rainSound;
        [Range(0, 100)] [SerializeField] private float rainVolume;
        [SerializeField] AudioClip thunderSound;
        [Range(0, 100)] [SerializeField] private float thunderVolume;
        [SerializeField] float fadeDuration;
        private AudioSource rainAudio;
        private AudioSource thunderAudio; 
        private bool canPlay = true;
        private bool isPlayingThunder;

        [Header("Frequency")] 
        [SerializeField] private bool autoControl = true;
        [SerializeField] public bool isRaining;
        [Range(0, 100)] [SerializeField] float rainFrequency;
        [Range(0, 100)] [SerializeField] float thunderFrequency;
        [SerializeField] float rainDuration = 5;                    // In minutes
        [SerializeField] float thunderInterval = 30;                // In seconds
        private bool running;
        
        [Header("Intensity")]
        [SerializeField] float rainDensity = 500;                           // The number of rain particles
                                                                     // Anything above 500 will cause heavy rain and
                                                                     // thunder; below and its just light rain
        private ParticleSystem rain;
        private FogController fog;

        void Awake() {
            try {
                // Find the AudioSource components
                rainAudio = GameObject.Find("RainSound").GetComponent<AudioSource>();
                thunderAudio = GameObject.Find("ThunderSound").GetComponent<AudioSource>();
                
                // Find the Particle system components
                rain = GameObject.Find("Rain").GetComponent<ParticleSystem>();
                
                fog = GetComponent<FogController>();
                
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
                thunderAudio.volume = thunderVolume / 100;
            } catch (UnassignedReferenceException) {
                Debug.LogError("RainController: One or both of the audio clips are missing");
                canPlay = false;
            }
        }
        
        void Update() {
            HandleParticles();
        
            // Set the number of emission for the rain particle
            var emission = rain.emission;
            emission.rateOverTime = rainDensity;
            
            // If we set the automatic control, and it is currently running
            if (autoControl && !running && !fog.isFoggy) {
                StartCoroutine(StartRain());
            }
            
            if (!rainAudio.isPlaying && canPlay && isRaining) {
                StartCoroutine(FadeAudioSource.StartFade(rainAudio, fadeDuration, rainVolume / 100));
                rainAudio.Play();
            }
            
            if (!isPlayingThunder) {
                StartCoroutine(StartThunder());
            }
        }

        private void HandleParticles() {
            // Stop the particle systems if the weather effect isnt raining
            if (!isRaining) {
                rain.Stop(false, ParticleSystemStopBehavior.StopEmitting);
    
                if (rainAudio.isPlaying) {
                    rainAudio.Stop();
                }
    
                if (thunderAudio.isPlaying) {
                    thunderAudio.Stop();
                }
            } else {
                rain.Play();
            }
        }

        void SetClips() {
            if (rainSound.Length == 0 || thunderSound == null) {
                throw new UnassignedReferenceException();
            }
            
            // If the intensity is set high, play the heavy sound effect with thunder
            if (rainDensity < 300) {
                rainAudio.clip = rainSound[0];
            } else {
                rainAudio.clip = rainSound[1];
                thunderAudio.clip = thunderSound;
            }
        }

        IEnumerator StartThunder() {
            isPlayingThunder = true;

            if (!thunderAudio.isPlaying && canPlay && isRaining) {
                if (UnityEngine.Random.Range(0, 100) < thunderFrequency) {
                    thunderAudio.Play();
                }
            }

            yield return new WaitForSeconds(thunderInterval);

            isPlayingThunder = false;
        }

        IEnumerator StartRain() {
            running = true;
            
            if (UnityEngine.Random.Range(0, 100) < rainFrequency) {
                isRaining = true;
                fog.isFoggy = true;
                Debug.Log("RainController: Starting rain");
            } else {
                isRaining = false;
                fog.isFoggy = false;
                Debug.Log("RainController: Stopped rain");
            }
            
            yield return new WaitForSeconds(rainDuration * 60);
            
            running = false;
        }
    }
}