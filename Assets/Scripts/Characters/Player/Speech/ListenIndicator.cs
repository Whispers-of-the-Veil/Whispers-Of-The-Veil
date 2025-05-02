using UnityEngine;

namespace Characters.Player.Speech {
    public class ListenIndicator : MonoBehaviour {
        [SerializeField] private GameObject emote;
        [SerializeField] private float range;
        
        void Start() {
            emote = transform.Find("Emote").gameObject;
            
            emote.SetActive(false);
        }

        void FixedUpdate() => emote.SetActive(CheckIfPlayerInRange());

        private bool CheckIfPlayerInRange() {
            Collider2D[] playerColider = Physics2D.OverlapCircleAll(transform.position, range, LayerMask.GetMask("player"));

            // Loop through all colliders and check line of sight
            foreach (Collider2D plaeyr in playerColider) {
                Vector2 directionToPlayer = (plaeyr.transform.position - transform.position).normalized;

                // Perform a 2D raycast to check for line of sight to the puzzle
                if (Physics2D.Raycast(transform.position, directionToPlayer, range, LayerMask.GetMask("player"))) {
                    return true;
                }
            }

            return false;
        }
    }
}