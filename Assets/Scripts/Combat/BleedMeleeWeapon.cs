//Owen Ingram

using Characters.NPC;
using UnityEngine;
using System.Collections.Generic;

namespace Combat
{
    public class BleedMeleeWeapon : MeleeWeapon
    {
        public int bleedDamage = 2;
        public float bleedDuration = 3f;
        public float bleedTickInterval = 1f;

        protected override void PerformAttackHits()
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

                        // Apply bleeding effect
                        BleedEffect bleed = enemy.GetComponent<BleedEffect>();
                        if (bleed == null)
                        {
                            bleed = enemy.gameObject.AddComponent<BleedEffect>();
                        }
                        bleed.bleedDamage = bleedDamage;
                        bleed.duration = bleedDuration;
                        bleed.tickInterval = bleedTickInterval;
                        bleed.ApplyBleed();
                    }
                }
            }
        }
    }
}