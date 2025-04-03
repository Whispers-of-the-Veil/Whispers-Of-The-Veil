// Lucas Davis(10-166)
//Owen Ingram(167-195)

using UnityEngine.AI;
using UnityEngine;
using System.Collections;
using Characters.Player;

namespace Characters.Enemy {
    public class EnemyController : MonoBehaviour
    {
        [Header("Combat")]
        [SerializeField] private float stoppingDistance = 2.0f;
        public float hurtDistance = 0.25f;
        private float timeOfLastAttack = 0;
        private bool hasStopped = false;
        private EnemyStats stats = null;
        
        [Header("Emotes")]
        private GameObject alertEmote;
        private GameObject angryEmote;
        private GameObject frustratedEmote;
        
        [Header("Movement")]
        [SerializeField] Transform target;
        [SerializeField] float speed = 2f;
        [SerializeField] float patrolRadius = 5f;
        [SerializeField] float minIvenstigateDistance; 
        [SerializeField] float maxIvenstigateDistance;
        [SerializeField] Transform boundedArea;
        public NavMeshAgent agent;
        
        [Header("Senses")]
        [SerializeField] public float sightRange;       // How far can this enemy see
        [SerializeField] public float hearingRange;     // How far can this enemy hear
        [SerializeField] public int heardLimit;
        private int heardCount;
        
        [Header("States")] 
        [SerializeField] float investigateDelay;
        [SerializeField] float patrolDelay;
        [SerializeField] float investigationTimeout;
        private bool isMoving = false;
        private bool isInvestigating = false;
        private bool isPatroling = false;
        
        private void Start() {
            if (target == null) {
                target = GameObject.Find("Player").GetComponent<Transform>();
            }
            
            alertEmote = GameObject.Find("Emotes/Alert");
            angryEmote = GameObject.Find("Emotes/Angry");
            frustratedEmote = GameObject.Find("Emotes/Frustrated");
            
            alertEmote.SetActive(false);
            angryEmote.SetActive(false);
            frustratedEmote.SetActive(false);
            
            // Navmesh agent
            agent = GetComponent<NavMeshAgent>();
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            
            GetReferences();
        }

        private void Update() {
            
        }
        
        private void FixedUpdate() {
            // If player is in sight, move to them
            if (CheckInSight() && !hasStopped) {
                StartCoroutine(ShowEmote(angryEmote));
                HasSeen();
            } else if (!isPatroling && !isInvestigating && !hasStopped) {
                StartCoroutine(DelayPatrol());
            }
        }

        
        /// <summary>
        /// Briefly display a given emote
        /// </summary>
        IEnumerator ShowEmote(GameObject emote) {
            emote.SetActive(true);
            yield return new WaitForSeconds(2);
            emote.SetActive(false);
        }
        
        // Attack -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// If the enemy has seen the player. It will move and engage in combat with them
        /// </summary>
        private void HasSeen() {
            Vector2 directionToPlayer = ((Vector2)target.position - (Vector2)transform.position).normalized;
            Vector2 stopPosition = (Vector2)target.position - (directionToPlayer * stoppingDistance);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(stopPosition, out hit, stoppingDistance, NavMesh.AllAreas)) {
                agent.SetDestination(hit.position);
                agent.speed = speed * 2;
            }

            float distanceToPlayer = Vector2.Distance(transform.position, target.position);
            if (distanceToPlayer <= hurtDistance) {
                if (Time.time >= timeOfLastAttack + stats.attackSpeed) {
                    timeOfLastAttack = Time.time;
                    AttackPlayer();
                    hasStopped = false; // Reset here so the enemy can move again after attacking
                }
            } else {
                hasStopped = false;
            }
        }



        
        /// <summary>
        /// Determine if the player can be seen by the enemy
        /// </summary>
        /// <returns>Returns true if they are; false otherwise</returns>
        private bool CheckInSight() {
            Collider2D[] puzzleColliders = Physics2D.OverlapCircleAll(transform.position, sightRange, LayerMask.GetMask("player"));

            foreach (Collider2D puzzleCollider in puzzleColliders) {
                Vector2 directionToPlayer = (puzzleCollider.transform.position - transform.position).normalized;

                if (Physics2D.Raycast(transform.position, directionToPlayer, sightRange, LayerMask.GetMask("player"))) {
                    //Debug.Log("Player detected in sight!");
                    return true;
                }
            }
            //Debug.Log("Player not detected in sight.");
            return false;
        }

        
        // Patrolling ---------------------------------------------------------------------------------------------------
        /// <summary>
        /// Delays the enemy starting their patrol
        /// </summary>
        IEnumerator DelayPatrol() {
            isPatroling = true;
            yield return new WaitForSeconds(patrolDelay);
            Patrol();
        }

        /// <summary>
        /// Get a random position around the entity and move it to that point
        /// </summary>
        private void Patrol() {
            Vector2 position;
            
            // If the boundedArea wasn't defined, randomly pick a position relative to the enemies position.
            // Otherwise, do so relative to the bounded area
            if (boundedArea == null) {
                position = (Vector2)transform.position + Random.insideUnitCircle * patrolRadius;
            }
            else {
                position = (Vector2)boundedArea.position + Random.insideUnitCircle * patrolRadius;
            }

            
            NavMeshHit hit;
        
            if (NavMesh.SamplePosition(position, out hit, patrolRadius, NavMesh.AllAreas)) {
                agent.SetDestination(hit.position);
                agent.speed = speed;
            }
            
            isPatroling = false;
        }
        
        // Investigate --------------------------------------------------------------------------------------------------
        
        /// <summary>
        /// Called outside this script (by the Voice.cs script). Checks if they player was within range of the entity
        /// when they spoke. If they were, the entity starts investigating the sound.
        /// </summary>
        public void VoiceDetected() {
            if (CheckInHeardRange()) {
                StartCoroutine(ShowEmote(alertEmote));

                if (!isInvestigating) {
                    StartCoroutine(DelayInvetigate());
                }
            }
        }
        
        /// <summary>
        /// Checks if they player is within hearing range of an entity
        /// </summary>
        /// <returns>True if they are; false otherwise</returns>
        private bool CheckInHeardRange() {
            Collider2D[] puzzleColliders = Physics2D.OverlapCircleAll(transform.position, hearingRange, LayerMask.GetMask("player"));

            // Loop through all colliders and check line of sight
            foreach (Collider2D puzzleCollider in puzzleColliders) {
                Vector2 directionToPlayer = (puzzleCollider.transform.position - transform.position).normalized;

                // Perform a 2D raycast to check for line of sight to the puzzle
                if (Physics2D.Raycast(transform.position, directionToPlayer, hearingRange, LayerMask.GetMask("player"))) {
                    return true;
                }
            }

            return false;            
        }

        /// <summary>
        /// Delays the entity Investigating the sound
        /// </summary>
        IEnumerator DelayInvetigate() {
            isInvestigating = true;
            yield return new WaitForSeconds(investigateDelay);
            InvestigateSound();
        }
        
        /// <summary>
        /// Get a random position just short of the player, and move the entity to it
        /// </summary>
        void InvestigateSound() {
            Vector2 directionToSound = ((Vector2)target.position - (Vector2)transform.position).normalized;
            float randomDistance = Random.Range(minIvenstigateDistance, maxIvenstigateDistance);
            Vector2 randomOffset = Random.insideUnitCircle * 1.5f;
            
            Vector2 targetPosition = (Vector2)transform.position + directionToSound * randomDistance + randomOffset;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, maxIvenstigateDistance, NavMesh.AllAreas)) {
                agent.SetDestination(hit.position);
                agent.speed = speed;
            }

            StartCoroutine(InvestigationTimeout());
        }
        
        /// <summary>
        /// Time out the investigation if the player wasnt found
        /// </summary>
        IEnumerator InvestigationTimeout() {
            yield return new WaitForSeconds(investigationTimeout);
            StartCoroutine(ShowEmote(frustratedEmote));
            isInvestigating = false;
            isPatroling = false;
        }
        
        private void AttackPlayer()
        {
            PlayerStats playerStats = target.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                stats.DealDamage(playerStats);
            }
        }
        
        private void GetReferences()
        {
            //get enemy stats
            stats = GetComponent<EnemyStats>();
        }
        
        //player to enemy combat
        public void TakeDamage(float damageAmount)
        {
            stats.TakeDamage(damageAmount); // Call TakeDamage from EnemyStats

        }
    }
}