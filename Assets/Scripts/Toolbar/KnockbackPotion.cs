using UnityEngine;
using Characters.Player;
using System.Collections;
using System.Collections.Generic;
using Characters.NPC;

public class KnockbackPotion : InventoryItemBase
{
    public float knockbackForce = 10f;
    public float knockbackDuration = 0.2f;
    public float effectRadius = 2f;
    public float holdDuration = 2f;
    public float bounceAmplitude = 0.1f;
    public float bounceSpeed = 5f;

    private Transform lastParent;
    private PlayerStats playerStats;

    private float holdTimer = 0f;
    private bool isConsuming = false;
    private Vector3 originalLocalPosition;

    public override string Name => "KnockbackPotion";

    private void Update()
    {
        if (transform.parent != null && transform.parent != lastParent)
        {
            lastParent = transform.parent;
            playerStats = transform.GetComponentInParent<PlayerStats>();
            originalLocalPosition = transform.localPosition;
        }

        bool isHeldByPlayer = playerStats != null && transform.IsChildOf(playerStats.transform);

        if (isHeldByPlayer)
        {
            if (Input.GetMouseButton(0))
            {
                holdTimer += Time.deltaTime;

                if (!isConsuming)
                {
                    isConsuming = true;
                }

                float bounceOffset = Mathf.Sin(Time.time * bounceSpeed) * bounceAmplitude;
                transform.localPosition = originalLocalPosition + Vector3.up * bounceOffset;

                if (holdTimer >= holdDuration)
                {
                    ApplyKnockback();
                    Destroy(gameObject);
                }
            }
            else
            {
                holdTimer = 0f;
                isConsuming = false;
                transform.localPosition = originalLocalPosition;
            }
        }
        else
        {
            holdTimer = 0f;
            isConsuming = false;
        }
    }

    private void ApplyKnockback()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, effectRadius);
        HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>();

        foreach (Collider2D enemy in hits)
        {
            if (enemy.CompareTag("Enemy") && !hitEnemies.Contains(enemy))
            {
                Enemy entity = enemy.GetComponent<Enemy>();
                Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();

                if (entity != null && rb != null)
                {
                    Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                    rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
                    StartCoroutine(ResetKnockback(rb));
                    hitEnemies.Add(enemy);
                }
            }
        }
    }

    private IEnumerator ResetKnockback(Rigidbody2D enemyRigidbody)
    {
        yield return new WaitForSeconds(knockbackDuration);
        if (enemyRigidbody != null)
        {
            enemyRigidbody.velocity = Vector2.zero;
        }
    }
}
