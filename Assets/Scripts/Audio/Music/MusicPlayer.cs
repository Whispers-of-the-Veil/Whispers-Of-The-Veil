// Lucas Davis

using System.Collections;
using Characters.NPC.BlackboardSystem;
using Characters.NPC.BlackboardSystem.Control;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Audio.Music {
    [RequireComponent(typeof (AudioSource))]

    public class MusicPlayer : MonoBehaviour {
        [SerializeField] private bool singleton;
        
        private MusicPlayer instance;
        
        [Header("Audio Clips")] 
        [SerializeField] AudioClip combatClip;
        [SerializeField] AudioClip[] clips;
        private AudioSource audioSource;

        [Header("Fade in")]
        // This is the duration for the fade in effect
        [SerializeField] float duration = 3f;
        // This variable should be between 0 and 1
        [SerializeField] float targetVolume;
        
        public BlackboardController controller {
            get => BlackboardController.instance;
        }

        Blackboard blackboard;
        BlackboardKey combatKey;

        private void Start() {
            if (singleton) {
                if (instance == null) {
                    instance = this;
                    DontDestroyOnLoad(this);
                }
                else {
                    Destroy(gameObject);
                }
            }
            
            blackboard = controller.GetBlackboard();
            combatKey = blackboard.GetOrRegisterKey("InCombat");
            
            audioSource = this.gameObject.GetComponent<AudioSource>();
            audioSource.volume = 0;

            audioSource.loop = false;
        }

        void Update() {
            if (blackboard.TryGetValue(combatKey, out bool value) && value) {
                if (audioSource.clip != combatClip || !audioSource.isPlaying) {
                    StartCoroutine(SwitchToClip(combatClip, duration, targetVolume));
                }
            } else {
                if ((audioSource.clip == null || audioSource.clip == combatClip || !audioSource.isPlaying) && clips.Length > 0) {
                    StartCoroutine(SwitchToClip(GetRandomClip(), duration, targetVolume));
                }
            }
        }
        
        private IEnumerator SwitchToClip(AudioClip newClip, float fadeDuration, float targetVolume) {
            // Fade out the current audio
            yield return StartCoroutine(FadeAudioSource.StartFade(audioSource, 1, 0));
    
            // Switch to the new clip
            audioSource.clip = newClip;
            audioSource.Play();
    
            // Fade in the new audio
            yield return StartCoroutine(FadeAudioSource.StartFade(audioSource, fadeDuration, targetVolume));
        }

        private AudioClip GetRandomClip() {
            return clips[Random.Range(0, clips.Length)];
        }
    }
}