using Characters.Player;
using Characters.Player.Voice;
using UnityEngine;
using UnityEngine.AI;

namespace Characters.Enemies.Behavior_Tree.Strategies.Conditional {
    public class Conditions {
        /// <summary>
        /// Check if a given entities position is within a defined range of a target
        /// </summary>
        public static bool InRange(Transform entity, float range) {
            Collider2D[] entites = Physics2D.OverlapCircleAll(entity.position, range, LayerMask.GetMask("player"));

            foreach (Collider2D player in entites) {
                Vector2 directionToPlayer = (player.transform.position - entity.position).normalized;

                if (Physics2D.Raycast(entity.position, directionToPlayer, range, LayerMask.GetMask("player"))) {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Determines if the NavMeshAgent has reached its target destination
        /// </summary>
        public static bool ReachedTarget(NavMeshAgent agent) {
            if (!agent.pathPending) {
                if (agent.remainingDistance <= agent.stoppingDistance) {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f) {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}