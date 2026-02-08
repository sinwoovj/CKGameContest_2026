using UnityEngine;

namespace Shurub
{
    public interface IInteractable
    {
        void Interact(PlayerController player);
        void UpdateProcess(float deltaTime);
        void InteractProcess();
    }
}
