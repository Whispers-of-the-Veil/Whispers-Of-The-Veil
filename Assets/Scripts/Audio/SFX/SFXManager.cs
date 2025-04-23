using System;
using System.Collections;
using UnityEngine;

namespace Audio.SFX {
    public class SFXManager : MonoBehaviour {
        public static SFXManager instance;
        
        [Header("Audio")]
        [SerializeField] private AudioSource SFXPlayer; // Reference to the SFXPlayerObject (Prefab)

        private void Awake() {
            if (instance == null) {
                instance = this;
                DontDestroyOnLoad(this);
            }
            else {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Instantiate a player to play the SFX clip, destroying the object once finished
        /// </summary>
        /// <param name="clip">The clip that we want to play</param>
        /// <param name="spawnPoint">The position to instantiate the player</param>
        /// <param name="volume">The volume for the player; default 1f</param>
        public void PlaySFX(AudioClip clip, Transform spawnPoint, float volume = 1f) {
            AudioSource source = Instantiate(SFXPlayer, spawnPoint.position, Quaternion.identity);
            
            source.clip = clip;
            source.volume = volume;
            float length = source.clip.length;
            
            source.Play();
            
            Destroy(source.gameObject, length);
        }
        
        public IEnumerator delaySFX(AudioClip clip, Transform spawnPoint, float delay, float volume = 1f) {
            yield return new WaitForSeconds(delay);
            PlaySFX(clip, spawnPoint, volume);
        }
    }
}