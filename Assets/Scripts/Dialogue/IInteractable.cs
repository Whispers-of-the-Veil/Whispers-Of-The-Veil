//Owen Ingram

using Characters.Player;
using UnityEngine;

namespace Dialogue
{
    public interface IInteractable
    {
        void Interact(PlayerController player);
    }
}