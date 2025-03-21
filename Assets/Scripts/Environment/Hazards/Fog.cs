using System;
using System.Collections;
using UnityEngine;
using Characters.Player;

namespace Environment.Hazards {
    public class Fog : MonoBehaviour {
        public bool isDamging = true;
        
        [Header("Damage")]
        [SerializeField] float damage = 0.5f; // The amount of damage the player takes
        [SerializeField] int interval = 2; // Damage interval in seconds
        private Coroutine damageCoroutine;

        [Header("Player")] 
        [SerializeField] GameObject player;
        private PlayerStats stats;
        private bool isInFog = false;
    
        void Start() {
            if (player == null)
            {
                player = GameObject.Find("Player");
            }
            this.stats = player.gameObject.GetComponent<PlayerStats>();
        }
    
        void Update() {
            // You can add additional logic here if needed.
        }

        private void OnTriggerEnter2D(Collider2D other) {
            Debug.Log("Entered Fog");
            if (isDamging && other.CompareTag(player.gameObject.tag) && !isInFog) {
                isInFog = true;
                damageCoroutine = StartCoroutine(EnvDamage());
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            Debug.Log("Exited Fog");
            if (isDamging && other.CompareTag(player.gameObject.tag)) {
                isInFog = false;

                if (damageCoroutine != null) {
                    StopCoroutine(damageCoroutine);
                    damageCoroutine = null;
                }
            }
        }

        IEnumerator EnvDamage() {
            while (isInFog) {
                Debug.Log("Applying damage to player in fog");
                this.stats.TakeDamage(damage);  // Apply damage to player

                // Update the health bar UI after damage
                if (this.stats is PlayerStats playerStats) {
                    playerStats.UpdateHealth();  // Call UpdateHealth() to refresh the health bar
                }

                yield return new WaitForSeconds(interval);  // Wait before applying damage again
            }
        }

    }
}
