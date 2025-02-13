// Lucas Davis(10-166)
//Owen Ingram(167-195)

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Characters.Player;

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
        [SerializeField] public GameObject alertEmote;
        private Rigidbody rb;
 
        //combat
        private float timeOfLastAttack = 0;
        private bool hasStopped = false;
        private EnemyStats stats = null;
        [SerializeField] Transform target;
        [SerializeField] private float stoppingDistance = 2.0f;
        public int health = 30;
        
        // Start is called before the first frame update
        private void Start() {
            this.alertEmote.SetActive(false);
            this.rb = GetComponent<Rigidbody>();
            GetReferences();
        }

        // Update is called once per frame
        private void Update() {
            this.playerInSight = CheckPlayerInSight();
            
            // If the enemy heard the player a set amount of times, they know where you are
            if (heardCount >= heardLimit) {
                alert = false; // reset the alert flag
                MoveEnemy(playerTransform.position);
            }
        }
        
        private void FixedUpdate() {
            // If player is in sight, move to them
            if (playerInSight && !hasStopped) {
                alert = false; // reset the alert flag
                MoveEnemy(playerTransform.position);
            }
            
            // If the enemy was alerted, look for the player
            if (alert && !isMoving) {
                StartCoroutine(Agitated());
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
                        this.alertEmote.SetActive(false);
                        return true;
                    }
                }
            }

            return false;
        }

        private void MoveEnemy(Vector3 targetPosition) {
            // Calculate direction and velocity towards the target
            direction = (targetPosition - transform.position).normalized;
            velocity = direction * speed;

            // Calculate the distance to the target
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

            // Move if the target is outside the stopping distance
            if (distanceToTarget > stoppingDistance) {
                rb.MovePosition(transform.position + velocity * Time.fixedDeltaTime);
                hasStopped = false; // Reset attack state when moving
            } else {
                // Attack logic if within stopping distance
                if (!hasStopped) {
                    timeOfLastAttack = Time.time;
                    hasStopped = true; // Enemy has stopped to attack
                }

                if (Time.time >= timeOfLastAttack + stats.attackSpeed) {
                    timeOfLastAttack = Time.time;
                    CharacterStats targetStats = target.GetComponent<CharacterStats>();
                    if (targetStats != null) {
                        AttackTarget(targetStats);
                        Debug.Log("attacking player");
                    }
                }
            }
        }
        
        // Reset enemy alert state
        private void ResetAlert() {
            this.alert = false;
            this.alertEmote.SetActive(false);
            heardCount = 0;
        }
        
        // Players voice was detected; change states alert and passive if they are within range
        public void VoiceDetected() {
            if (Physics.CheckSphere(transform.position, hearingRange, player)) {
                this.alertEmote.SetActive(true);
                this.alert = true;
                
                heardCount++;

                if (!IsInvoking(nameof(ResetAlert))) {
                    Invoke(nameof(ResetAlert), alertDuration);
                }
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

        void Die()
        {
            Debug.Log("Enemy defeated");
            Destroy(gameObject);
        }
        
        private void AttackTarget(CharacterStats statsToDamage)
        {
            //perform player damage to enemy stats script
            Debug.Log("attacking players");
            stats.DealDamage(statsToDamage);
        }

        private void GetReferences()
        {
            //get enemy stats
            stats = GetComponent<EnemyStats>();
        }
        
        //player to enemy combat
        public void TakeDamage(int damage)
        {
            //take damage amount from enemy health, if <= 0 then enemy dies
            health -= damage;
            Debug.Log($"Enemy hit! Remaining health: {health}");
            if (health <= 0)
            {
                Die();
            }
        }
    }
}