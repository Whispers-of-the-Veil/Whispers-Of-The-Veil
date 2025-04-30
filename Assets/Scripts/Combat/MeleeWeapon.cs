using System.Collections;
using System.Collections.Generic;
using Audio.SFX;
using UnityEngine;
using Characters.NPC;

namespace Combat
{
    public class MeleeWeapon : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private AudioClip attackSFX;
        private SFXManager sfxManager {
            get => SFXManager.instance;
        }
        
        [Header("Combat Settings")]
        public int damage = 10;
        public float attackCooldown = 0.5f;
        public float knockbackForce = 3f;
        public float knockbackDuration = 0.2f;

        [Header("Swing Settings")]
        public float swingAngle = 45f;
        public float swingDuration = 0.2f;

        private bool canAttack = true;
        private bool isSwinging = false;
        private bool isEquipped = false;

        private Quaternion originalRotation;

        private void Awake()
        {
            originalRotation = transform.localRotation;
        }

        public void EquipWeapon()
        {
            isEquipped = true;
            canAttack = true;
            isSwinging = false;
            transform.localRotation = originalRotation;
            StopAllCoroutines();
        }

        public void UnequipWeapon()
        {
            isEquipped = false;
            isSwinging = false;
            transform.localRotation = originalRotation;
            StopAllCoroutines();
        }

        public void Attack()
        {
            if (!isEquipped)
            {
                Debug.Log("Weapon not equipped.");
                return;
            }

            if (!canAttack)
            {
                Debug.Log("Weapon is on cooldown.");
                return;
            }

            if (isSwinging)
            {
                Debug.Log("Weapon is already swinging.");
                return;
            }

            Debug.Log("Weapon attack triggered.");
            sfxManager.PlaySFX(attackSFX, transform, 1f);
            StartCoroutine(SwingWeapon());
            canAttack = false;
            StartCoroutine(AttackCooldown());
            StartCoroutine(HandleAttackHit());
        }


        private IEnumerator HandleAttackHit()
        {
            yield return new WaitForSeconds(0.1f);
            PerformAttackHits();
        }

        private void PerformAttackHits()
        {
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

        private IEnumerator SwingWeapon()
        {
            isSwinging = true;
            float timer = 0f;
            float halfDuration = swingDuration / 2f;

            // Swing forward
            while (timer < halfDuration)
            {
                float angle = Mathf.Lerp(0, swingAngle, timer / halfDuration);
                transform.localRotation = originalRotation * Quaternion.Euler(0f, 0f, angle);
                timer += Time.deltaTime;
                yield return null;
            }

            // Swing back
            timer = 0f;
            while (timer < halfDuration)
            {
                float angle = Mathf.Lerp(swingAngle, 0f, timer / halfDuration);
                transform.localRotation = originalRotation * Quaternion.Euler(0f, 0f, angle);
                timer += Time.deltaTime;
                yield return null;
            }

            transform.localRotation = originalRotation;
            isSwinging = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}