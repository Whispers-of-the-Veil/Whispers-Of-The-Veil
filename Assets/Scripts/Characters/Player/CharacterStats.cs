using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters.Player
{
    public class CharacterStats : MonoBehaviour
    {
        
        [SerializeField] protected int health;
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
        
        public void TakeDamage(int damage)
        {
            int healthAfterDamage = health - damage;
            SetHealthTo(healthAfterDamage);
            Debug.Log($"Player took {damage} damage. Health left: {health}");
        }
        
        public virtual void InitVariables()
        {
            maxHealth = 100;
            SetHealthTo(maxHealth);
            isDead = false;
        }
    }
}
