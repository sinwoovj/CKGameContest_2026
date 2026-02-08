using System;
using UnityEngine;
using UnityEngine.UI;

namespace Shurub
{
    public class InteractionUI : Singleton<InteractionUI>
    {
        [SerializeField] private Slider progressBar;

        private void Start()
        {
            Hide();    
        }

        public void UpdateUI(Structure structure)
        {
            progressBar.value = structure.Progress;
            
            switch (structure.state)
            {
                case InteractionState.InProgress:
                    Show();
                    break;
                case InteractionState.Idle:
                case InteractionState.Success:
                case InteractionState.Failed:
                case InteractionState.Cancelled:
                    Hide();
                    break;
            }
        }

        private void Show()
        {
            progressBar.gameObject.SetActive(true);
        }
        private void Hide()
        {
            progressBar.gameObject.SetActive(false);
        }

    }
}