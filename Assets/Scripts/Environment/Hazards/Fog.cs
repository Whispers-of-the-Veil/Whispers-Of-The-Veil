using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Characters.Player;

namespace Environment.Hazards {
    public class Fog : MonoBehaviour {
        public bool isDamging = true;
        
        [Header("Damage")]
        [SerializeField] int damage = 10; // The amount of damage the player takes
        [SerializeField] int interval = 2; // Damage interval in seconds
        private Coroutine damageCoroutine;

        [Header("Player")] 
        [SerializeField] GameObject player;
        private CharacterStats stats;
        private bool isInFog = false;
    
        void Start() {
            this.stats = player.gameObject.GetComponent<CharacterStats>();
        }
    
        void Update() {
            
        }

        private void OnTriggerEnter(Collider other) {
            if (isDamging && other.CompareTag(player.gameObject.tag) && !isInFog) {
                isInFog = true;
                damageCoroutine = StartCoroutine(EnvDamage());
            }
        }

        private void OnTriggerExit(Collider other) {
            if (isDamging && other.CompareTag(player.gameObject.tag)) {
                isInFog = false;

                if (damageCoroutine != null) {
                    StopCoroutine(damageCoroutine);

                    damageCoroutine = null;
                }
            }
        }

        IEnumerator EnvDamage() {
            while(isInFog) {
                this.stats.TakeDamage(damage);

                yield return new WaitForSeconds(interval);
            }
        }
    }
}