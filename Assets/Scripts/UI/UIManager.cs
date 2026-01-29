using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Shurub
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField]
        public GameObject pausePanel;
        public bool isPause;

        static public UIManager Instance;

        void Start()
        {
            Instance = this;    
        }

        void Update()
        {
        
        }

        // Button Fucntions...
    
        public void SetPausePanel()
        {
            isPause = !isPause;
            pausePanel.SetActive(isPause);
            PlayerController.Instance.playerInput.enabled = !isPause;
        }

        public void OpenPausePanel()
        {
            if (!isPause)
            {
                isPause = true;
                pausePanel.SetActive(isPause);
                PlayerController.Instance.playerInput.enabled = !isPause;
            }
        }

        public void ClosePausePanel()
        {
            if (isPause)
            {
                isPause = false;
                pausePanel.SetActive(isPause);
                PlayerController.Instance.playerInput.enabled = !isPause;
            }
        }

        public void GoToPlayerManagement()
        {

        }

        public void GoToSettings()
        {

        }
    }
}
