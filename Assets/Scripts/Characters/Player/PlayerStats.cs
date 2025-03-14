using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Characters.Player
{
    public class PlayerStats : MonoBehaviour
    {
        [SerializeField] private float health = 3f;
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private bool isDead;
        private Image[] hearts;
        [SerializeField] private Sprite fullHeart;
        [SerializeField] private Sprite halfHeart;
        [SerializeField] private Sprite emptyHeart;

        private void Start()
        {
            health = maxHealth;
            FindHearts();
            UpdateHealth();
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
            FindHearts();
            UpdateHealth();
        }

        private void FindHearts()
        {
            GameObject hud = GameObject.FindWithTag("HUD"); 
            if (hud != null)
            {
                Transform healthCanvas = hud.transform.Find("HealthCanvas");
                if (healthCanvas != null)
                {
                    hearts = healthCanvas.GetComponentsInChildren<Image>();
                }
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                TakeDamage(0.5f);
            }
        }

        public void UpdateHealth()
        {
            if (hearts == null) return;

            for (int i = 0; i < hearts.Length; i++)
            {
                if (health >= i + 1)
                {
                    hearts[i].sprite = fullHeart;
                }
                else if (health >= i + 0.5f)
                {
                    hearts[i].sprite = halfHeart;
                }
                else
                {
                    hearts[i].sprite = emptyHeart;
                }
            }
        }

        public void TakeDamage(float damage)
        {
            health -= damage;
            health = Mathf.Clamp(health, 0f, maxHealth);
            Debug.Log($"Player took {damage} damage. Remaining health: {health}");

            UpdateHealth();

            if (health <= 0)
            {
                Die();
            }
        }

        public void Die()
        {
            isDead = true;
            CheckpointManager.Instance.RespawnPlayer(gameObject);
            health = maxHealth;
            UpdateHealth();
            Debug.Log("Player died, respawning");
        }
    }
}
