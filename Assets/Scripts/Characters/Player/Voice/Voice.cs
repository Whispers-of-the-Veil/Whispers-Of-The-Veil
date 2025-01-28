using Characters.Enemy;
using UnityEngine;

namespace Characters.Player.Voice {
    public class Voice : MonoBehaviour {
        [SerializeField] public GameObject[] enemies; // An array of enemy gameobjects that can hear the player voice

        [Header("Components")] 
        private AudioSource _audioSource;
        
        private void Start() {
            this._audioSource = this.gameObject.GetComponent<AudioSource>();
        }

        private void Update() {
            
        }
        
        public void Detect() {
            foreach (var enemy in enemies) {
                enemy.GetComponent<EnemyController>().VoiceDetected();
            }
        }
    }
}