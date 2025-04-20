//Test script

using UnityEngine;

namespace Characters.Enemies
{
    public class EnemyTest : MonoBehaviour
    {
        [Header("Detection Settings")] public float detectionRange = 5f;
        public LayerMask playerLayer;

        [Header("Movement Settings")] public float moveSpeed = 2f;

        private Transform player;
        private Rigidbody2D rb;
        private Vector2 movement;
        private bool isChasing = false;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        void Update()
        {
            DetectPlayer();
        }

        void FixedUpdate()
        {
            if (isChasing)
            {
                MoveTowardsPlayer();
            }
        }

        void DetectPlayer()
        {
            Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);

            if (playerCollider != null)
            {
                player = playerCollider.transform;
                isChasing = true;
            }
            else
            {
                isChasing = false;
            }
        }

        void MoveTowardsPlayer()
        {
            if (player == null) return;

            // Get direction to player
            Vector2 direction = (player.position - transform.position).normalized;
            movement = direction * moveSpeed;

            // Move enemy
            rb.velocity = movement;
        }

        void OnDrawGizmosSelected()
        {
            // Draw detection range in the editor
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
    }
}