using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Characters.NPC;

namespace Combat
{
    public class MeleeWeapon : MonoBehaviour
    {
        public int damage = 10;
        public float attackCooldown = 0.5f;
        public float knockbackForce = 3f;
        public float knockbackDuration = 0.2f;

        private bool canAttack = true;

        public void Attack()
        {
            if (!canAttack)
            {
                return;
            }
            
            canAttack = false;
            StartCoroutine(AttackCooldown());

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.5f);
            HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>();

            foreach (Collider2D enemy in hits)
            {
                if (enemy.CompareTag("Enemy") && !hitEnemies.Contains(enemy))
                {
                    Enemy entity = enemy.GetComponent<Enemy>();
                    Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();

                    if (entity != null && rb != null)
                    {
                        entity.TakeDamage(damage);
                        hitEnemies.Add(enemy);

                        Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                        rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);

                        StartCoroutine(ResetKnockback(rb));
                    }
                }
            }
        }

        private IEnumerator AttackCooldown()
        {
            yield return new WaitForSeconds(attackCooldown);
            canAttack = true;
        }

        private IEnumerator ResetKnockback(Rigidbody2D enemyRigidbody)
        {
            yield return new WaitForSeconds(knockbackDuration);
            if (enemyRigidbody != null)
            {
                enemyRigidbody.velocity = Vector2.zero;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
