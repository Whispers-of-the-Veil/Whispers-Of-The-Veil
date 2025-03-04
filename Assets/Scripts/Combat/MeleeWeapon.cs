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
        public int durability = 10;

        private bool canAttack = true;
        private bool isAttacking = false;

        private Collider2D weaponCollider;

        void Awake()
        {
            weaponCollider = GetComponent<Collider2D>();
            weaponCollider.enabled = false;
        }

        void Update()
        {
            CheckForAttack();
        }
        
        private void CheckForAttack()
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

            weaponCollider.enabled = true;

            Invoke(nameof(ResetAttack), 0.1f);
            Invoke(nameof(EnableCooldown), attackCooldown);
        }

        void ResetAttack()
        {
            weaponCollider.enabled = false;
            isAttacking = false;
        }

        void EnableCooldown()
        {
            canAttack = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isAttacking && other.CompareTag("Enemy"))
            {
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);

                    Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
                    if (enemyRb != null)
                    {
                        Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
                        enemyRb.AddForce(knockbackDirection * knockbackStrength, ForceMode2D.Impulse);
                    }

                    durability--;
                    if (durability <= 0)
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }
    }
}
