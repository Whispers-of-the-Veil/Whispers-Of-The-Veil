using System.Collections;
using UnityEngine;
using Characters.Enemy;

namespace Combat
{
    public class MeleeWeapon : MonoBehaviour
    {
        public int damage = 10;
        public float attackCooldown = 0.5f;
        public float knockbackForce = 3f; // Strength of knockback
        public float knockbackDuration = 0.2f; // Time the knockback lasts
        public int durability = 5; // Starting durability of the weapon

        private bool canAttack = true;

        private void Update()
        {
            // Check if the weapon is being held (has a parent) and the player clicks to attack
            if (transform.parent != null && Input.GetMouseButtonDown(0) && canAttack)
            {
                Attack();
            }
        }

        public void Attack()
        {
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
                        
                        // Calculate knockback direction and apply force
                        Vector2 knockbackDirection = enemy.transform.position - transform.position;
                        knockbackDirection.Normalize(); // Make sure the direction is consistent
                        enemyRigidbody.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
                        
                        StartCoroutine(ApplyKnockbackDuration(enemyRigidbody, enemy)); // Apply knockback for a limited time
                        StartCoroutine(AttackCooldown());
                    }

                    // Decrease durability and destroy the weapon if it's out of durability
                    durability--;
                    if (durability <= 0)
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }

        private IEnumerator ApplyKnockbackDuration(Rigidbody2D enemyRigidbody, Collider2D enemy)
        {
            // Apply knockback for a limited duration, after which the force will stop
            float timer = 0f;
            while (timer < knockbackDuration)
            {
                if (enemy == null || enemyRigidbody == null)
                {
                    yield break; // Exit the coroutine if the enemy or Rigidbody is destroyed
                }

                timer += Time.deltaTime;
                yield return null;
            }

            // Once duration is over, stop the knockback by applying no further force
            if (enemyRigidbody != null)
            {
                enemyRigidbody.velocity = Vector2.zero;
            }
        }

        private IEnumerator AttackCooldown()
        {
            canAttack = false;
            yield return new WaitForSeconds(attackCooldown);
            canAttack = true;
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize attack range in the editor
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
