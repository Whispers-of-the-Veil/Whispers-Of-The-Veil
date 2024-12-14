using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Characters.Player;

namespace Characters.Enemy
{
    public class EnemyStats : CharacterStats
    {
        [SerializeField] private int damage;
        [SerializeField] private bool canAttack;
        public float attackSpeed;
        
        private void Start()
        {
            InitVariables();
        }

        public void DealDamage(CharacterStats statsToDamage)
        {
            statsToDamage.TakeDamage(damage);
        }
        
        public override void Die()
        {
            base.Die();
            Destroy(gameObject);
        }
        
        public override void InitVariables()
        {
            maxHealth = 20;
            SetHealthTo(maxHealth);
            isDead = false;

            int dmg = damage;
            attackSpeed = 1.5f;
            canAttack = true;
        }
    }
}
