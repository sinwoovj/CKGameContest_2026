using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Shurub
{
    public class UIManager : MonoBehaviourPun
    {
        [SerializeField]
        public GameObject pausePanel;
        [SerializeField]
        public GameObject HPUIObj;
        [SerializeField]
        public TextMeshProUGUI playTimeText;

        public bool isPause;

        static public UIManager Instance;

        void Start()
        {
            Instance = this;
            InitHPUI();
            InitTimeUI();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) UIManager.Instance.SetPausePanel();
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

        public void InitHPUI()
        {
            HPUIObj.GetComponent<Slider>().maxValue = GameManager.Instance.maxHp;
            HPUIObj.GetComponent<Slider>().value = GameManager.Instance.maxHp;
        }

        public void UpdateHPUI()
        {
            HPUIObj.GetComponent<Slider>().value = (float)PhotonNetwork.CurrentRoom.CustomProperties[GameManager.HP_KEY];
        }

        public void InitTimeUI()
        {
            playTimeText.text = "00 : 00";
        }

        public void UpdateTimeUI()
        {
            float time = (float)PhotonNetwork.CurrentRoom.CustomProperties[GameManager.PLAYTIME_KEY];
            playTimeText.text = $"{(int)(time / 60):00} : {(int)(time % 60):00}";
        }
    }
}
