using UnityEngine;

namespace Dialogue
{
    public class HelperHintZone : MonoBehaviour
    {
        [SerializeField] private DialogueObject hintDialogue;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                HelperHintController.Instance.SetCurrentHint(hintDialogue);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (HelperHintController.Instance.CurrentHint == hintDialogue)
                {
                    HelperHintController.Instance.ClearCurrentHint();
                }
            }
        }
    }
}