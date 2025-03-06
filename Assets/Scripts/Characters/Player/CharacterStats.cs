//Owen Ingram

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters.Player
{
    public class CharacterStats : MonoBehaviour
    {
        
        [SerializeField] protected float health = 3f; // Use float to allow fractional health
        [SerializeField] protected int maxHealth;
        [SerializeField] protected bool isDead;
        // Start is called before the first frame update
        void Start()
        {
            InitVariables();
        }

        public virtual void CheckHealth()
        {
            if (health <= 0)
            {
                health = 0;
                Die();
            }

            if (health >= maxHealth)
            {
                health = maxHealth;
            }
        }
        
        public virtual void Die()
        {
            isDead = true;
        }
        
        public void SetHealthTo(int healthToSetTo)
        {
            health = healthToSetTo;
            CheckHealth();
        }
        
        public void TakeDamage(float damage)
        {
            health -= damage;
            health = Mathf.Clamp(health, 0f, 3f); // Ensure health stays within bounds (0 to 3)
            Debug.Log($"Player took {damage} damage. Remaining health: {health}");
        }

        
        public virtual void InitVariables()
        {
            maxHealth = 3;
            SetHealthTo(maxHealth);
            isDead = false;
        }
        
    }
}
