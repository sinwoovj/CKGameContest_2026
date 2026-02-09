using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shurub
{
    public class GUIManager : Singleton<GUIManager>
    {
        protected override bool CheckDontDestroyOnLoad()
        {
            return false;
        }

        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private GameObject HPUIObj;
        [SerializeField] private TextMeshProUGUI playTimeText;
        [SerializeField] private TextMeshProUGUI totalTimeText;

        public GameObject ResultPanel => resultPanel;

        public bool IsPaused { get; private set; }

        void Start()
        {
            InitHPUI();
            InitTimeUI();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && NetworkManager.Instance.CurrentRoomState == GameState.Playing)
            {
                SetPausePanel();
            }
        }

        // Button Fucntions...

        public void SetPausePanel()
        {
            IsPaused = !IsPaused;
            pausePanel.SetActive(IsPaused);
            GameManager.Instance.LocalPlayer.GetComponent<PlayerController>().playerInput.enabled = !IsPaused;
        }

        public void OpenPausePanel()
        {
            if (!IsPaused)
            {
                IsPaused = true;
                pausePanel.SetActive(IsPaused);
                GameManager.Instance.LocalPlayer.GetComponent<PlayerController>().playerInput.enabled = !IsPaused;
            }
        }

        public void ClosePausePanel()
        {
            if (IsPaused)
            {
                IsPaused = false;
                pausePanel.SetActive(IsPaused);
                GameManager.Instance.LocalPlayer.GetComponent<PlayerController>().playerInput.enabled = !IsPaused;
            }
        }

        public void RetryGame()
        {
            NetworkManager.Instance.SetGameState(GameState.Retry);
        }

        public void GoToPlayerManagement()
        {

        }

        public void GoToSettings()
        {

        }

        public void GoToMain()
        {
            GameManager.Instance.GoToMain();
        }

        public void InitHPUI()
        {
            HPUIObj.GetComponent<Slider>().interactable = false;
            HPUIObj.GetComponent<Slider>().maxValue = GameManager.Instance.maxHp;
            HPUIObj.GetComponent<Slider>().value = GameManager.Instance.maxHp;
        }

        public void UpdateHPUI()
        {
            HPUIObj.GetComponent<Slider>().value = GameManager.Instance.GetHP();
        }

        public void InitTimeUI()
        {
            playTimeText.text = "00 : 00";
        }

        public void UpdateTimeUI()
        {
            float time = GameManager.Instance.GetPlayTime();
            playTimeText.text = $"{(int)(time / 60):00} : {(int)(time % 60):00}";
        }

        public void UpdateTotalTimeUI()
        {
            float time = GameManager.Instance.GetPlayTime();
            totalTimeText.text = $"[Total Time: {(int)(time / 60):00}:{(int)(time % 60):00}]";
        }
    }
}
