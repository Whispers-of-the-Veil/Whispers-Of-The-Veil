using System.Collections;
using System.Collections.Generic;
using Characters.Player;
using UnityEngine;

namespace Dialogue
{
    public class DialogueActivator : MonoBehaviour, IInteractable
    {
        [SerializeField] private DialogueObject dialogueObject;

        public void UpdateDialogueObject(DialogueObject dialogueObject)
        {
            this.dialogueObject = dialogueObject;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && other.TryGetComponent(out PlayerController player))
            {
                player.Interactable = this;
                Interact(player); // Automatically trigger dialogue when the player enters the collider
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
            player.DialogueUI.ShowDialogue(dialogueObject);
        }
    }
}