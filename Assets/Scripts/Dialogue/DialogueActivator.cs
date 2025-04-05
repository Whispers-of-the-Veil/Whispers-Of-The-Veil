using System.Collections;
using System.Collections.Generic;
using Characters.Player;
using UnityEngine;

namespace Dialogue
{
    public class DialogueActivator : MonoBehaviour, IInteractable
    {
        [SerializeField] private DialogueObject dialogueObject;
        private bool hasInteracted = false;
        private PlayerController currentPlayer;


        public void UpdateDialogueObject(DialogueObject dialogueObject)
        {
            this.dialogueObject = dialogueObject;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasInteracted) return;

            if (other.CompareTag("Player") && other.TryGetComponent(out PlayerController player))
            {
                player.Interactable = this;
                Interact(player);
            }
        }


        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player") && other.TryGetComponent(out PlayerController player))
            {
                if (player.Interactable is DialogueActivator dialogueActivator && dialogueActivator == this)
                {
                    player.Interactable = null;
                }
            }
        }

        public void Interact(PlayerController player)
        {
            foreach (DialogueResponseEvents responseEvents in GetComponents<DialogueResponseEvents>())
            {
                if (responseEvents.DialogueObject == dialogueObject)
                {
                    player.DialogueUI.AddResponseEvents(responseEvents.Events);
                    break;
                }
            }

            player.DialogueUI.OnDialogueFinished += HandleDialogueFinished;
            player.DialogueUI.ShowDialogue(dialogueObject);
        }
        
        private void HandleDialogueFinished()
        {
            if (currentPlayer != null)
            {
                currentPlayer.DialogueUI.OnDialogueFinished -= HandleDialogueFinished;
                currentPlayer = null;
            }

            hasInteracted = true;
            this.enabled = false;
        }
        
        private void DisableAfterDialogue()
        {
            hasInteracted = true;
            currentPlayer.DialogueUI.OnDialogueFinished -= DisableAfterDialogue;
            gameObject.SetActive(false);
        }
    }

}