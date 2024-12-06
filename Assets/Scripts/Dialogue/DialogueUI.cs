using UnityEngine;
using TMPro;
using System.Collections;

namespace Dialogue
{
    public class DialogueUI : MonoBehaviour
    {
        [SerializeField] private GameObject dialogueBox;
        [SerializeField] private TMP_Text textLabel;
        
        public bool IsOpen { get; private set; }
        
        private ResponseHandler responseHandler;
        private TypeWriteEffect typeWriteEffect;

        private void Start()
        {
            typeWriteEffect = GetComponent<TypeWriteEffect>();
            responseHandler = GetComponent<ResponseHandler>();
            CloseDialogueBox();
        }

        public void ShowDialogue(DialogueObject dialogueObject)
        {
            IsOpen = true;
            dialogueBox.SetActive(true);
            StartCoroutine(StepThroughDialogue(dialogueObject));
        }

        public void AddResponseEvents(ResponseEvent[] responseEvents)
        {
            responseHandler.AddResponseEvents(responseEvents);
        }

        private IEnumerator StepThroughDialogue(DialogueObject dialogueObject)
        {
            for (int i = 0; i < dialogueObject.Dialogue.Length; i++)
            {
                string dialogue = dialogueObject.Dialogue[i];
                yield return RunTypingEffect(dialogue);
                
                textLabel.text = dialogue;
                
                if (i == dialogueObject.Dialogue.Length - 1 && dialogueObject.HasResponses) break;

                yield return null;
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
            }

            if (dialogueObject.HasResponses)
            {
                responseHandler.ShowResponses(dialogueObject.Responses);
            }
            else
            {
                CloseDialogueBox();
            }
        }

        private IEnumerator RunTypingEffect(string dialogue)
        {
            typeWriteEffect.Run(dialogue, textLabel);

            while (typeWriteEffect.IsRunning)
            {
                yield return null;

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    typeWriteEffect.Stop();
                }
            }
        }

        public void CloseDialogueBox()
        {
            IsOpen = false;
            dialogueBox.SetActive(false);
            textLabel.text = string.Empty;
        }
    }
}