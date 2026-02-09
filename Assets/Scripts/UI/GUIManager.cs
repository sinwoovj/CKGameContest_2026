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
        [SerializeField] private TextMeshProUGUI countDownText;

        public GameObject ResultPanel => resultPanel;

        // CountDown Variables...
        private string[] countDownStr = { "시작", "1", "2", "3" };
        private bool isCountDown = false;
        private float countDownTime = 0f;
        private int countDownNum = 0;

        public bool IsPaused { get; private set; }

        void Start()
        {
            InitUI();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && NetworkManager.Instance.CurrentRoomState == GameState.Playing)
            {
                SetPausePanel();
            }
            if (isCountDown)
            {
                countDownTime += -Time.deltaTime;
                if (countDownTime <= 0f)
                {
                    EndCountDownUI();
                }
                else
                {
                    ScaleCountDownUI();
                    if ((int)countDownTime != countDownNum)
                    {
                        UpdateCountDownUI();
                    }
                }
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

        public void InitUI()
        {
            InitCountDownUI();
            InitHPUI();
            InitTimeUI();
        }

        public void CountDownUI()
        {
            InitCountDownUI();
            countDownText.gameObject.SetActive(true);
            isCountDown = true;
        }
        private void InitCountDownUI()
        {
            countDownText.gameObject.SetActive(false);
            countDownTime = countDownStr.Length;
            countDownNum = countDownStr.Length - 1;
            isCountDown = false;
            countDownText.text = countDownStr[countDownNum];
        }

        private void ScaleCountDownUI()
        {
            float t = countDownTime % 1;
            float v = Mathf.Abs(Mathf.Sin(t * Mathf.PI));

            countDownText.gameObject.transform.localScale = new Vector3(v,v,1);
        }
        private void UpdateCountDownUI()
        {
            countDownText.text = countDownStr[--countDownNum];
        }

        private void EndCountDownUI()
        {
            isCountDown = false;
            countDownText.gameObject.SetActive(false);
            NetworkManager.Instance.SetGameState(GameState.Playing);
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
