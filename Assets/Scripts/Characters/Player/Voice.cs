using Characters.Enemy;
using UnityEngine;

namespace Characters.Player {
    public class Voice : MonoBehaviour {
        [SerializeField] public GameObject[] enemies; // An array of enemy gameobjects that can hear the player voice

        public void Detect() {
            foreach (var enemy in enemies) {
                enemy.GetComponent<EnemyController>().VoiceDetected();
            }
        }
    }
}