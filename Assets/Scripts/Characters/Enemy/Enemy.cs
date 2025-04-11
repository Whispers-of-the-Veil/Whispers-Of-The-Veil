// Lucas Davis(10-166)
//Owen Ingram(167-195)

using UnityEngine.AI;
using UnityEngine;
using System.Collections;
using Characters.Player;
using Audio.SFX;

// This is a general class that is attached to every enemy. It doesnt provided any behavior logic, but
// it has conditions and actions that every enemy will have access to.
namespace Characters.Enemy {
    public class Enemy : MonoBehaviour {
        [Header("Audio")]
        [SerializeField] AudioClip idleSfx;
        [SerializeField] AudioClip walkSfx;
        [SerializeField] AudioClip alertSfx;
        [SerializeField] AudioClip angrySfx;
        [SerializeField] AudioClip deathSfx;
        private SFXManager sfxManager {
            get => SFXManager.instance;
        }
        
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
        [SerializeField] Transform patrolArea;
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
        
        /// <summary>
        /// Briefly display a given emote
        /// </summary>
        IEnumerator ShowEmote(GameObject emote) {
            emote.SetActive(true);
            yield return new WaitForSeconds(2);
            emote.SetActive(false);
        }
        
        /// <summary>
        /// Determine if the player can be seen by the enemy
        /// </summary>
        /// <returns>Returns true if they are; false otherwise</returns>
        private bool Seen() {
            if (target.GetComponent<PlayerStats>().isInvisible)
                return false;
            
            Collider2D[] collider = Physics2D.OverlapCircleAll(transform.position, sightRange, LayerMask.GetMask("player"));

            foreach (Collider2D puzzleCollider in collider) {
                Vector2 directionToPlayer = (puzzleCollider.transform.position - transform.position).normalized;

                if (Physics2D.Raycast(transform.position, directionToPlayer, sightRange, LayerMask.GetMask("player"))) {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Checks if they player is within hearing range of an entity
        /// </summary>
        /// <returns>True if they are; false otherwise</returns>
        private bool HeardNoise() {
            if (target.GetComponent<PlayerStats>().isInvisible)
                return false;
            
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