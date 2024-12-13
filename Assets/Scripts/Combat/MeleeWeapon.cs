using System.Collections;
using System.Collections.Generic;
using Characters.Enemy;
using UnityEngine;

namespace Combat
{
    public class MeleeWeapon : MonoBehaviour
    {
        public int damage = 10; 
        public float attackCooldown = 0.5f; 
        private bool canAttack = true;

        void Update()
        {
            if (Input.GetMouseButtonDown(0) && canAttack)
            {
                Attack();
            }
        }

        void Attack()
        {
            canAttack = false;

            GetComponent<MeshCollider>().enabled = true;

            Invoke(nameof(ResetAttack), 0.1f);
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