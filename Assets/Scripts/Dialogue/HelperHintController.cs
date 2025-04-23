using UnityEngine;
using Characters.Player;

namespace Dialogue
{
    public class HelperHintController : MonoBehaviour
    {
        public static HelperHintController Instance { get; private set; }

        [SerializeField] private PlayerController player;
        [SerializeField] private GameObject helperSprite;

        public DialogueObject CurrentHint { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (helperSprite != null)
                helperSprite.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H) && CurrentHint != null)
            {
                ShowHint();
            }
        }

        public void SetCurrentHint(DialogueObject hint)
        {
            CurrentHint = hint;
        }

        public void ClearCurrentHint()
        {
            CurrentHint = null;
        }

        private void ShowHint()
        {
            if (helperSprite != null)
                helperSprite.SetActive(true);

            player.DialogueUI.ShowDialogue(CurrentHint);
            player.DialogueUI.OnDialogueFinished += HideHelper;
        }

        private void HideHelper()
        {
            if (helperSprite != null)
                helperSprite.SetActive(false);

            player.DialogueUI.OnDialogueFinished -= HideHelper;
        }
    }
}