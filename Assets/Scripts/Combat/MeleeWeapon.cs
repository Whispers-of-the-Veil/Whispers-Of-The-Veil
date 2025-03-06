using System.Collections;
using UnityEngine;
using Characters.Enemy;

namespace Combat
{
    public class MeleeWeapon : MonoBehaviour
    {
        public int damage = 10;
        public float attackCooldown = 0.5f;
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
                    if (enemyController != null)
                    {
                        enemyController.TakeDamage(damage);
                        StartCoroutine(AttackCooldown());
                    }
                }
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