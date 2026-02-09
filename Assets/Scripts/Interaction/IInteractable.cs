using UnityEngine;

namespace Shurub
{
    public interface IInteractable
    {
        void Interact(PlayerController player);
        void InteractProcess(int playerViewId);
        void RequestCancel(int playerViewId);
    }
}
