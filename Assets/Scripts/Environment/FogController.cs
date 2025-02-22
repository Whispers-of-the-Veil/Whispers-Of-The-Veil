using System;
using System.Collections;
using UnityEngine;

namespace Environment  {
    public class FogController : MonoBehaviour {
        [Header("Frequency")] 
        [SerializeField] private bool autoControl = true;
        [SerializeField] public bool isFoggy;
        [Range(0, 100)] [SerializeField] private float fogFrequency;
        [SerializeField] private float fogDuration = 5;
        private bool running;
        
        [Header("Intensity")]
        [SerializeField] private float fogDensity = 10;
        private ParticleSystem fog;
        private RainController rain;
        
        void Awake() {
            fog = GameObject.Find("Fog").GetComponent<ParticleSystem>();
            rain = GetComponent<RainController>();
        }

        void Update() {
            HandleParticles();
            
            // Set the number of emission for the rain particle
            var emission = fog.emission;
            emission.rateOverTime = fogDensity;
            
            if (autoControl && !running && !rain.isRaining) {
                StartCoroutine(StartFog());
            }
        }

        private void HandleParticles() {
            // Stop the particle systems if the weather effect isnt raining
            if (!isFoggy) {
                fog.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            } else {
                fog.Play();
            }
        }
        
        IEnumerator StartFog() {
            running = true;
            
            if (UnityEngine.Random.Range(0, 100) < fogFrequency) {
                isFoggy = true;
                Debug.Log("RainController: Starting rain");
            } else {
                isFoggy = false;
                Debug.Log("RainController: Stopped rain");
            }
            
            yield return new WaitForSeconds(fogDuration * 60);
            
            running = false;
        }
    }
}