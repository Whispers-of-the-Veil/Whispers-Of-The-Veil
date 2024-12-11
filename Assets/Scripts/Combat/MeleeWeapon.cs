using System.Collections;
using System.Collections.Generic;
using Characters.Enemy;
using UnityEngine;

namespace Combat
{
    public class MeleeWeapon : MonoBehaviour
    {
        public int damage = 10; // Amount of damage dealt
        public float attackCooldown = 0.5f; // Cooldown between attacks

        private bool canAttack = true;

        void Update()
        {
            // Left-click to attack
            if (Input.GetMouseButtonDown(0) && canAttack)
            {
                Attack();
            }
        }

        void Attack()
        {
            canAttack = false;

            // Temporarily enable the sword's collider for the attack
            GetComponent<MeshCollider>().enabled = true;

            // Reset attack state and disable the collider after the attack duration
            Invoke(nameof(ResetAttack), 0.1f); // Adjust duration as needed
            Invoke(nameof(EnableCooldown), attackCooldown);
        }

        void ResetAttack()
        {
            GetComponent<MeshCollider>().enabled = false;
        }

        void EnableCooldown()
        {
            canAttack = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            // Check if the object hit is an enemy
            if (other.CompareTag("Enemy"))
            {
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
            }
        }
    }
}