using System;
using System.Collections;
using UnityEngine;

namespace Characters.Enemy {
    public class EnemyController : MonoBehaviour {
        [Header("States")] 
        [SerializeField] public float alertDuration;
        private bool alert, playerInSight;

        [Header("Movement")] 
        [SerializeField] public Transform playerTransform;
        [SerializeField] public float speed;
        [SerializeField] public float radius;
        private Vector3 direction, velocity;
        private bool isMoving = false;

        [Header("Senses")]
        [SerializeField] public LayerMask player;
        [SerializeField] public float sightRange;       // How far can this enemy see
        [SerializeField] public float hearingRange;     // How far can this enemy hear
        [SerializeField] public int heardLimit;
        private RaycastHit hit;
        private Vector3 directionToPlayer, randomPosition;
        private int heardCount;
        
        [Header("Components")]
        private Rigidbody rb;
        
        // Start is called before the first frame update
        private void Start() {
            this.rb = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        private void Update() {
            this.playerInSight = CheckPlayerInSight();
        }
        
        private void FixedUpdate() {
            // If player is in sight, move to them
            if (playerInSight) {
                alert = false; // reset the alert flag
                MoveEnemy(playerTransform);
            }
            
            // If the enemy was alerted, look for the player
            if (alert && !isMoving) {
                StartCoroutine(Agitated());
            }
            
            // If the enemy heard the player 5 times, they know where you are
            if (heardCount == heardLimit) {
                alert = false; // reset the alert flag
                MoveEnemy(playerTransform);
            }
        }

        private Vector3 GetRandomDirection() {
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * radius;
            randomDirection.y = 0;
            
            return transform.position + randomDirection;
        }

        // Check if the player is within sight
        private bool CheckPlayerInSight() {
            // If the player is within the enemies sight Range
            if (Physics.CheckSphere(transform.position, sightRange, player)) {
                directionToPlayer = (playerTransform.position - transform.position).normalized;

                // Perform a raycast to check if there is a direct line of sight to the player
                if (Physics.Raycast(transform.position, directionToPlayer, out hit, sightRange)) {
                    // Check if the raycast hit the player
                    if (hit.transform.CompareTag("Player")) {
                        return true;
                    }
                }
            }

            return false;
        }

        private void MoveEnemy(Transform target) {
            MoveEnemy(target.position);
        }

        private void MoveEnemy(Vector3 targetPosition) {
            direction = (targetPosition - transform.position).normalized;
            velocity = direction * speed;
            
            rb.MovePosition(transform.position + velocity * Time.fixedDeltaTime);
        }
        
        // Reset enemy alert state
        private void ResetAlert() {
            this.alert = false;
        }
        
        // Players voice was detected; change states alert and passive if they are within range
        public void VoiceDetected() {
            if (Physics.CheckSphere(transform.position, hearingRange, player)) {
                this.alert = true;
                heardCount++;
                Invoke(nameof(ResetAlert), alertDuration);
            }
        }

        IEnumerator Agitated() {
            isMoving = true;
            
            randomPosition = GetRandomDirection();
            
            // Move the enemy towards the random position until it reaches the target
            while (Vector3.Distance(transform.position, randomPosition) > 0.1f) {
                MoveEnemy(randomPosition);
                yield return new WaitForFixedUpdate(); // Wait for the next physics update
            }
            
            yield return new WaitForSeconds(2);
            
            isMoving = false;
        }
    }
}