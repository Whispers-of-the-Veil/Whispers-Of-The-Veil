using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Characters.Player;

namespace menu
{
    public class DeathScreenUI : MonoBehaviour
    {
        public static DeathScreenUI Instance;

        [Header("UI References")] public GameObject deathScreenPanel;
        public Button respawnButton;
        public Button mainMenuButton;
        public Button loadSaveButton;

        [Header("Gameplay References")] public GameObject player;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("DeathScreenUI instance created.");
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            HideDeathScreen();
            FindPlayerReference();

            var stats = player?.GetComponent<PlayerStats>();
            if (stats != null && stats.health <= 0)
            {
                stats.health = stats.maxHealth;
                stats.UpdateHealth();
            }
        }


        private void Start()
        {
            Debug.Log("DeathScreenUI Start called");

            deathScreenPanel.SetActive(false);

            respawnButton.onClick.AddListener(RespawnAtCheckpoint);
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            loadSaveButton.onClick.AddListener(OpenLoadMenu);

            FindPlayerReference();
        }
        
        private void FindPlayerReference()
        {
            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player");
            }
        }
        
        public void ShowDeathScreen()
        {
            FindPlayerReference();
            deathScreenPanel.SetActive(true);
            Time.timeScale = 0f;
        }

        public void HideDeathScreen()
        {
            deathScreenPanel.SetActive(false);
            Time.timeScale = 1f;
        }

        private void RespawnAtCheckpoint()
        {
            HideDeathScreen();

            CheckpointManager.Instance.RespawnPlayer(player);

            // Restore player health
            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.health = stats.maxHealth;
                stats.UpdateHealth();
            }
            else
            {
                Debug.LogWarning("PlayerStats component not found on player.");
            }
        }
        
        private void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Main Menu");
        }

        private void OpenLoadMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("SaveFiles");
        }
    }
}