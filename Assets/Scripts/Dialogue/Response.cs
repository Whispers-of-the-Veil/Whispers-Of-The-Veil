using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dialogue
{
    [System.Serializable]
    public class Response
    {
        [SerializeField] private string reponseText;
        [SerializeField] private DialogueObject dialogueObject;
        
        public string ResponseText => reponseText;
        
        public DialogueObject DialogueObject => dialogueObject;
    }
}