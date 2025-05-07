//Owen Ingram

using System.Collections;
using System.Collections.Generic;
using Characters.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dialogue
{
    public class DialogueActivator : MonoBehaviour, IInteractable
    {
        [SerializeField] private DialogueObject dialogueObject;
        private bool hasInteracted = false;
        private PlayerController currentPlayer;
        public DialogueActivator currentActivator;

        
        private void OnEnable()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;

            if (currentPlayer != null)
            {
                currentPlayer.DialogueUI.OnDialogueFinished -= HandleDialogueFinished;
            }
        }

        private void OnSceneUnloaded(Scene scene)
        {
            currentActivator = null;
        }



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
            currentPlayer = player;
            
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