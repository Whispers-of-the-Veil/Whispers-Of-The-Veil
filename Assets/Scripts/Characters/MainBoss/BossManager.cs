using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Audio.SFX;
using Management;

namespace Characters.MainBoss {
    public class BossManager : MonoBehaviour {
        public static BossManager instance;

        [Header("Boss")]
        [SerializeField] GameObject bossPrefab;
        [SerializeField] Transform spawnPoint;
        [SerializeField] float duration = 3; // duration in minutes

        private GameObject currentBossInstance;
        public bool bossSpawned = false;
        public bool inSpawnPhase = false;
        private float timer = 0f;
        private float totalCycleDuration => duration * 2 * 60f; // 6 minutes
        private float halfCycleDuration => duration * 60f;      // 3 minutes

        private string townMainSceneName = "Town_Main";
        private Scene currentScene;
        
        public DontDestroyManager dontDestroyManager {
            get => DontDestroyManager.instance;
        }
        
        void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
        void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            currentScene = scene;

            if (scene.name == townMainSceneName) {
                if (ShouldSpawnBoss() && !bossSpawned) {
                    SpawnBoss();
                }
            } else {
                if (currentBossInstance != null) {
                    Destroy(currentBossInstance);
                    currentBossInstance = null;
                }
                bossSpawned = false; // <- important reset
            }
        }
        
        void Awake() {
            if (instance == null) {
                instance = this;
                dontDestroyManager.Track(this.gameObject);
            }
            else {
                Destroy(gameObject);
            }

            currentScene = SceneManager.GetActiveScene();

            
        }
        
        void Update() {
            bool shouldSpawnNow = ShouldSpawnBoss();

            if (shouldSpawnNow != inSpawnPhase) {
                inSpawnPhase = shouldSpawnNow;
            }

            if (currentScene.name != townMainSceneName) return;

            if (shouldSpawnNow && !bossSpawned) {
                SpawnBoss();
            }
            else if (!shouldSpawnNow && bossSpawned) {
                DespawnBoss();
            }
        }
        
        void FixedUpdate() {
            timer += Time.fixedDeltaTime;
    
            if (timer >= totalCycleDuration) {
                timer = 0f;
            }
        }

        private bool ShouldSpawnBoss() => timer % totalCycleDuration < halfCycleDuration;

        private void SpawnBoss() {
            if (bossPrefab == null) return;
            if (spawnPoint == null) spawnPoint = GameObject.Find("BossSpawnPoint").transform;

            currentBossInstance = Instantiate(bossPrefab, spawnPoint.position, Quaternion.identity);
            bossSpawned = true;
        }

        private void DespawnBoss() {
            if (currentBossInstance != null) {
                Destroy(currentBossInstance);
                currentBossInstance = null;
            }
            bossSpawned = false;
        }
        
        private void OnDestroy() {
            if (instance == this) instance = null;
        }
    }
}