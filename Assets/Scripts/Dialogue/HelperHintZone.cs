using UnityEngine;

namespace Dialogue
{
    public class HelperHintZone : MonoBehaviour
    {
        [SerializeField] private DialogueObject hintDialogue;
        [SerializeField] private GameObject echoIndicatorPrefab;
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                HelperHintController.Instance.SetCurrentHint(hintDialogue);
                echoIndicatorPrefab.SetActive(true);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (HelperHintController.Instance.CurrentHint == hintDialogue)
                {
                    HelperHintController.Instance.ClearCurrentHint();
                    echoIndicatorPrefab.SetActive(false);
                }
            }
        }
    }
}