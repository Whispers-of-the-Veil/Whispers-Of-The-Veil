// Lucas Davis

using UnityEngine;

namespace Audio.Music {
    [RequireComponent(typeof (AudioSource))]

    public class MusicPlayer : MonoBehaviour {
        [Header("Audio Clips")]
        [SerializeField] AudioClip[] clips;
        private AudioSource audioSource;

        [Header("Fade in")]
        // This is the duration for the fade in effect
        [SerializeField] float duration;
        // This variable should be between 0 and 1
        [SerializeField] float targetVolume;

        private void Start() { 
            audioSource = this.gameObject.GetComponent<AudioSource>();
            audioSource.volume = 0;

            audioSource.loop = false;
        }

        private void Update() {
            if (!audioSource.isPlaying && (clips.Length != 0 || clips == null)) {
                audioSource.clip = GetRandomClip();
                StartCoroutine(FadeAudioSource.StartFade(audioSource, duration, targetVolume));
                audioSource.Play();
            }
        }

        private AudioClip GetRandomClip() {
            return clips[Random.Range(0, clips.Length)];
        }
    }
}