//Owen Ingram

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Dialogue
{
    public class DialogueUI : MonoBehaviour
    {
        [SerializeField] private GameObject dialogueBox;
        [SerializeField] private TMP_Text textLabel;
        [SerializeField] private Image npcImage;
        public static DialogueUI instance;
        public event Action OnDialogueFinished;

        public bool IsOpen { get; private set; }
        
        private ResponseHandler responseHandler;
        private TypeWriteEffect typeWriteEffect;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            instance = null;
        }

        private void Start()
        {
            typeWriteEffect = GetComponent<TypeWriteEffect>();
            responseHandler = GetComponent<ResponseHandler>();
            
            CloseDialogueBox();
        }

        public void ShowDialogue(DialogueObject dialogueObject)
        {
            IsOpen = true;
            if (dialogueBox != null)
            {
                dialogueBox.SetActive(true);
            }

            if (npcImage != null)
            {
                npcImage.sprite = dialogueObject.NpcSprite;
                npcImage.gameObject.SetActive(true);
            }

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
            if (npcImage != null)
            {
                npcImage.gameObject.SetActive(false);
            }
            OnDialogueFinished?.Invoke();
        }
    }
}
