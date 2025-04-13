using Characters.Player;
using UnityEngine;

namespace Characters.Enemies.Behavior_Tree.Strategies.Conditional {
    public class Conditions {
        /// <summary>
        /// Check if a given entities position is within a defined range of a target
        /// </summary>
        public static bool InRange(Transform entity, float range) {
            Collider2D[] collider = Physics2D.OverlapCircleAll(entity.position, range, LayerMask.GetMask("player"));

            foreach (Collider2D puzzleCollider in collider) {
                Vector2 directionToPlayer = (puzzleCollider.transform.position - entity.position).normalized;

                if (Physics2D.Raycast(entity.position, directionToPlayer, range, LayerMask.GetMask("player"))) {
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Check if noise was made; running sounds, player is talking, etc.
        /// </summary>
        public static bool HeardSound() {
            return false;
        }
    }
}