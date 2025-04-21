using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Characters.Player;

namespace Characters.NPC
{
    public class EnemyStats : MonoBehaviour
    {
        [SerializeField] private float health;
        [SerializeField] private float damage = 1f;
        [SerializeField] private bool canAttack = true;
        public float attackSpeed = 1.5f;
        public bool isDead;

        private void Start()
        {
            isDead = false;
        }

        public void DealDamage(PlayerStats playerStats)
        {
            if (playerStats != null)
            {
                playerStats.TakeDamage(damage);
                playerStats.UpdateHealth();
                Debug.Log("Enemy attacked the player!");
            }
        }

        public void TakeDamage(float damageAmount)
        {
            health -= damageAmount;
            Debug.Log($"Enemy took {damageAmount} damage. Remaining health: {health}");

            if (health <= 0)
            {
                Die();
            }
        }

        public void Die()
        {
            isDead = true;
            Debug.Log("Enemy defeated");
            Destroy(gameObject);
        }
    }
}