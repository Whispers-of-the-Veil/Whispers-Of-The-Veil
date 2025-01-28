//Owen Ingram

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
        public float knockbackStrength = 5f;
        private bool canAttack = true;
        private bool isAttacking = false;

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
            isAttacking = true;

            GetComponent<MeshCollider>().enabled = true;

            Invoke(nameof(ResetAttack), 0.1f);
            Invoke(nameof(EnableCooldown), attackCooldown);
        }

        void ResetAttack()
        {
            GetComponent<MeshCollider>().enabled = false;
            isAttacking = false;
        }

        void EnableCooldown()
        {
            canAttack = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isAttacking && other.CompareTag("Enemy"))
            {
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);

                    // Apply knockback
                    Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
                    if (enemyRb != null)
                    {
                        Vector3 knockbackDirection = (other.transform.position - transform.position).normalized;
                        enemyRb.AddForce(knockbackDirection * knockbackStrength, ForceMode.Impulse);
                    }
                }
            }
        }
    }

}