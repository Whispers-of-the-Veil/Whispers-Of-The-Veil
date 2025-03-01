//Owen Ingram

using UnityEngine;

namespace Dialogue
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Dialogue/DialogueObject")]
    public class DialogueObject : ScriptableObject
    {
        [SerializeField] [TextArea] private string[] dialogue;
        [SerializeField] private Response[] responses;
        [SerializeField] private Sprite npcSprite;

        
        public string[] Dialogue => dialogue;
        public bool HasResponses => Responses != null && Responses.Length > 0;
        public Response[] Responses => responses;
        public Sprite NpcSprite => npcSprite;

    }
}
