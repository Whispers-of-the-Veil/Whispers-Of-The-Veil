using System.Collections;
using UnityEngine;
using Characters.Enemy;

namespace Combat
{
    public class MeleeWeapon : MonoBehaviour
    {
        public int damage = 10;
        public float attackCooldown = 0.5f;
        public float knockbackForce = 3f;
        public float knockbackDuration = 0.2f; // Time the knockback lasts
        public int durability = 5; // Starting durability of the weapon

        private bool isAttacking = false; // Combined flag for attack state and cooldown

        private void Update()
        {
            // Check if the weapon is being held (has a parent) and the player clicks to attack
            if (transform.parent != null && Input.GetMouseButtonDown(0) && !isAttacking)
            {
                Attack();
            }
        }

        public void Attack()
        {
            // Prevent attack if already in the attacking state (either in cooldown or just attacked)
            isAttacking = true;

            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, 0.5f);
            foreach (Collider2D enemy in hitEnemies)
            {
                if (enemy.CompareTag("Enemy"))
                {
                    EnemyController enemyController = enemy.GetComponent<EnemyController>();
                    Rigidbody2D enemyRigidbody = enemy.GetComponent<Rigidbody2D>();

                    if (enemyController != null && enemyRigidbody != null)
                    {
                        enemyController.TakeDamage(damage);

                        // Apply knockback force immediately
                        Vector2 knockbackDirection = (enemy.transform.position - transform.position).normalized;
                        enemyRigidbody.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

                        // Start knockback coroutine
                        StartCoroutine(ApplyKnockbackAndDestroy(enemyRigidbody, enemy));
                    }
                }
            }

            // Start cooldown coroutine after attack
            StartCoroutine(AttackCooldown());
        }

        private IEnumerator ApplyKnockbackAndDestroy(Rigidbody2D enemyRigidbody, Collider2D enemy)
        {
            yield return new WaitForSeconds(knockbackDuration); // Wait for knockback duration

            // Ensure enemy stops moving after knockback
            if (enemyRigidbody != null)
            {
                enemyRigidbody.velocity = Vector2.zero;
            }

            // Re-enable enemy movement if it was stopped
            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.agent.isStopped = false;
                enemyController.agent.ResetPath(); // Reset pathfinding to avoid getting stuck
            }

            // Decrease durability only once, ensure it happens only if not already destroyed
            if (durability > 0)
            {
                durability--; // Decrease durability by 1
            }

            // Destroy the weapon if durability reaches 0
            if (durability <= 0)
            {
                Destroy(gameObject);
            }
        }

        private IEnumerator AttackCooldown()
        {
            yield return new WaitForSeconds(attackCooldown);
            isAttacking = false; // Reset the attack state after cooldown
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}