using System;
using System.Collections;
using Audio;
using Audio.SFX;
using UnityEngine;
using Characters.Player;
using Random = System.Random;

namespace Environment.Hazards {
    public class Fog : MonoBehaviour {
        public bool isDamging = true;

        [Header("Audio")] 
        [SerializeField] AudioClip distortionSfx;
        [SerializeField] AudioClip spawnSfx;
        private AudioSource source;
        private Coroutine fadeCoroutine;
        private SFXManager sfxManager {
            get => SFXManager.instance;
        }
        
        [Header("Damage")]
        [SerializeField] float damage = 0.5f; // The amount of damage the player takes
        [SerializeField] int interval = 2; // Damage interval in seconds
        private Coroutine damageCoroutine;
        
        [Header("Monster")]
        [SerializeField] GameObject monsterPrefab;
        [SerializeField] float spawnTimer;
        [SerializeField] [Range(0f, 100f)] float spawnChance;
        private bool dontSpawn;
        private bool hasSpawned;
        private Coroutine spawnCoroutine;
        private Random random = new Random();

        [Header("Player")] 
        [SerializeField] GameObject player;
        private PlayerStats stats;
        private bool isInFog = false;
    
        void Start() {
            dontSpawn = false;
            hasSpawned = false;
            
            source = GetComponent<AudioSource>();
            source.clip = distortionSfx;
            source.volume = 0;
            source.loop = true;
            
            source.Play();
            
            if (player == null) {
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
                
                if (fadeCoroutine != null) {
                    StopCoroutine(fadeCoroutine);
                }
                fadeCoroutine = StartCoroutine(FadeAudioSource.StartFade(source, 4, 1));

                if (!hasSpawned && !dontSpawn) {
                    spawnCoroutine = StartCoroutine(SpawnMonster());
                }
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

                if (fadeCoroutine != null) {
                    StopCoroutine(fadeCoroutine);
                }
                fadeCoroutine = StartCoroutine(FadeAudioSource.StartFade(source, 2, 0));

                if (spawnCoroutine != null) {
                    StopCoroutine(spawnCoroutine);
                    spawnCoroutine = null;
                }
            }
        }

        IEnumerator SpawnMonster() {
            if (random.NextDouble() < (spawnChance / 100)) {
                Debug.Log("Spawning monster!");
                
                yield return new WaitForSeconds(spawnTimer);
                sfxManager.PlaySFX(spawnSfx, transform, 1f);
                yield return new WaitForSeconds(1f);
                
                GameObject monster = Instantiate(monsterPrefab, transform.position, Quaternion.identity);
                hasSpawned = true;
            }
            else {
                Debug.Log("Monster wont spawn here");
                dontSpawn = true;
            }
        }

        IEnumerator EnvDamage() {
            while (isInFog) {
                Debug.Log("Applying damage to player in fog");
                this.stats.TakeDamage(damage);

                if (this.stats is PlayerStats playerStats) {
                    playerStats.UpdateHealth();
                }

                yield return new WaitForSeconds(interval);
            }
        }

    }
}
