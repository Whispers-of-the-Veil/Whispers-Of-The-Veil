// Lucas Davis(10-166)
//Owen Ingram(167-195)

using UnityEngine.AI;
using UnityEngine;
using System.Collections;
using Characters.Player;


// This is a general class that is attached to every enemy. It doesnt provided any behavior logic, but
// it has conditions and actions that every enemy will have access to.
namespace Characters.Enemies {
    public class Enemy : MonoBehaviour {
        
        private EnemyStats stats = null;
        
        private void Start() {
            GetReferences();
        }
        
        private void GetReferences()
        {
            //get enemy stats
            stats = GetComponent<EnemyStats>();
        }
        
        public void TakeDamage(float damageAmount)
        {
            stats.TakeDamage(damageAmount); // Call TakeDamage from EnemyStats
        }

        
        // /// <summary>
        // /// If the enemy has seen the player. It will move and engage in combat with them
        // /// </summary>
        // private void HasSeen() {
        //     Vector2 directionToPlayer = ((Vector2)target.position - (Vector2)transform.position).normalized;
        //     Vector2 stopPosition = (Vector2)target.position - (directionToPlayer * stoppingDistance);
        //
        //     NavMeshHit hit;
        //     if (NavMesh.SamplePosition(stopPosition, out hit, stoppingDistance, NavMesh.AllAreas)) {
        //         agent.SetDestination(hit.position);
        //         agent.speed = speed * 2;
        //     }
        //
        //     float distanceToPlayer = Vector2.Distance(transform.position, target.position);
        //     if (distanceToPlayer <= hurtDistance) {
        //         if (Time.time >= timeOfLastAttack + stats.attackSpeed) {
        //             timeOfLastAttack = Time.time;
        //             AttackPlayer();
        //             hasStopped = false; // Reset here so the enemy can move again after attacking
        //         }
        //     } else {
        //         hasStopped = false;
        //     }
        // }
        //
        // /// <summary>
        // /// Get a random position just short of the player, and move the entity to it
        // /// </summary>
        // void InvestigateSound() {
        //     Vector2 directionToSound = ((Vector2)target.position - (Vector2)transform.position).normalized;
        //     float randomDistance = Random.Range(minIvenstigateDistance, maxIvenstigateDistance);
        //     Vector2 randomOffset = Random.insideUnitCircle * 1.5f;
        //     
        //     Vector2 targetPosition = (Vector2)transform.position + directionToSound * randomDistance + randomOffset;
        //     
        //     NavMeshHit hit;
        //     if (NavMesh.SamplePosition(targetPosition, out hit, maxIvenstigateDistance, NavMesh.AllAreas)) {
        //         agent.SetDestination(hit.position);
        //         agent.speed = speed;
        //     }
        //
        //     StartCoroutine(InvestigationTimeout());
        // }
        

        

    }
}