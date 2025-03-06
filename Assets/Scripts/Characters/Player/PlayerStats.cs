using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Characters.Player
{
    public class PlayerStats : CharacterStats
    {
        [SerializeField] private Image[] hearts;
        [SerializeField] private Sprite fullHeart;
        [SerializeField] private Sprite halfHeart;
        [SerializeField] private Sprite emptyHeart;

        private void Start()
        {
            InitVariables();
            UpdateHealth();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                TakeDamage(0.5f);  // Apply half-heart damage for testing
                UpdateHealth();
            }
        }

        public void UpdateHealth()
        {
            for (int i = 0; i < hearts.Length; i++)
            {
                if (health >= i + 1) // Full heart
                {
                    hearts[i].sprite = fullHeart;
                }
                else if (health >= i + 0.5f) // Half heart
                {
                    hearts[i].sprite = halfHeart;
                }
                else // Empty heart
                {
                    hearts[i].sprite = emptyHeart;
                }
            }
        }

        public void TakeDamage(float damage)
        {
            health -= damage;
            health = Mathf.Clamp(health, 0f, hearts.Length); // Ensure health stays within bounds
            Debug.Log($"Player took {damage} damage. Remaining health: {health}");
        }

        public override void Die()
        {
            isDead = true;
            CheckpointManager.Instance.RespawnPlayer(gameObject);
            health = hearts.Length; // Reset to full health
            Debug.Log("Player died, respawning");
        }
    }
}