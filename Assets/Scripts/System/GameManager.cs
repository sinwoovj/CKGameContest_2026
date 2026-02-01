using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Shurub
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        public static GameManager Instance;

        public enum GameState
        {
            None = 0,

            Boot,          // 게임 실행 직후
            Lobby,         // 로비 (룸 선택 전)
            Loading,       // 씬/리소스 로딩
            Ready,         // 카운트다운
            Playing,       // 실제 플레이
            Paused,        // 일시정지
            Result,        // 결과 화면
            Retry,         // 재시작
            GameOver       // 완전 종료
        } 
        public GameState state = GameState.None;

        // Room Property Key
        public const string HP_KEY = "GlobalHP";
        public const string STATE_KEY = "GameState";
        public const string PLAYTIME_KEY = "PlayTime";

        // Room Properties
        public float maxHp = 100f;
        public float currentHp = 0f;
        public float playTime = 0f;

        // Logic of Hp Decrease
        float hpTickTimer;
        const float HP_TICK_INTERVAL = 2f; // per 2s
        const int HP_DECREASE = -1;
        const float HP_SUBMIT_REWARD = 5f;

        // Monohaviour Functions
        private void Awake()
        {
            Instance = this;
        }
        void Start()
        {
            SetGameState(GameState.Boot);
            SetGameState(GameState.Lobby);
            SetGameState(GameState.Loading);
            //로비에서부터 준비 시작-> 로딩 -> 레디 -> 이후 플레잉으로 진행, 하지만 지금은 생략
        }

        private void Update()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            if (state == GameState.Playing)
            {
                hpTickTimer += Time.deltaTime;
                if (hpTickTimer >= HP_TICK_INTERVAL)
                {
                    hpTickTimer = 0f;
                    DecreaseHP();
                }
                playTime += Time.deltaTime;
                if (Time.frameCount % 30 == 0) // Do not send it too often
                {
                    SetPlayTime(playTime);
                }
            }
        }

        // Private Functions

        private void SetPlayerInput(bool val)
        {
            PlayerManager.LocalPlayerInstance.GetComponent<PlayerInput>().enabled = val;
        }

        private void OpenResultUI()
        {
            GUIManager.Instance.UpdateTotalTimeUI();
            GUIManager.Instance.resultPanel.SetActive(true);
        }

        private void CloseResultUI()
        {
            GUIManager.Instance.resultPanel.SetActive(false);
        }

        // Public Functions

        public void OnGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Loading:
                    PlayerSpawner.Instance.ReSpawnPlayer();
                    SetHP(maxHp);
                    playTime = 0f;
                    SetPlayTime(playTime);
                    CloseResultUI();
                    SetPlayerInput(true);
                    PlayerManager.LocalPlayerInstance.GetComponent<Animator>().SetTrigger("Default");
                    IngredientManager.Instance.ClearIngredient();
                    TestManager.Instance.InstantiateTest();
                    SetGameState(GameState.Ready);
                    break;
                case GameState.Ready:
                    //카운트 다운
                    SetGameState(GameState.Playing);
                    break;
                case GameState.Playing:
                    break;
                case GameState.Result:
                    OpenResultUI();
                    SetPlayerInput(false);
                    break;
                case GameState.Retry:
                    SetGameState(GameState.Loading);
                    break;
            }
        }

        public void SetHP(float value)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable
            {
                { HP_KEY, value }
            };

            //ExitGames.Client.Photon.Hashtable ht = (ExitGames.Client.Photon.Hashtable)PhotonNetwork.CurrentRoom.CustomProperties.Clone();
            //ht[HP_KEY] = value;

            PhotonNetwork.CurrentRoom.SetCustomProperties(ht);
        }

        public void ChangeHP(float delta)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            float curHp = (float)PhotonNetwork.CurrentRoom.CustomProperties.Get(HP_KEY);
            curHp += delta;

            ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable
            {
                { HP_KEY, curHp }
            };

            //ExitGames.Client.Photon.Hashtable ht = (ExitGames.Client.Photon.Hashtable)PhotonNetwork.CurrentRoom.CustomProperties.Clone();
            //ht[HP_KEY] = curHp;

            PhotonNetwork.CurrentRoom.SetCustomProperties(ht);
        }

        public void SubmitFood()
        {
            ChangeHP(HP_SUBMIT_REWARD);
        }

        public void DecreaseHP()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            float hp = (float)PhotonNetwork.CurrentRoom.CustomProperties.Get(HP_KEY);
            hp += HP_DECREASE;

            ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable
            {
                { HP_KEY, hp }
            };

            //ExitGames.Client.Photon.Hashtable ht = (ExitGames.Client.Photon.Hashtable)PhotonNetwork.CurrentRoom.CustomProperties.Clone();
            //ht[HP_KEY] = HP_DECREASE;

            PhotonNetwork.CurrentRoom.SetCustomProperties(ht);

            if (hp <= 0)
            {
                SetGameState(GameState.Result);
            }
        }

        public void SetGameState(GameState newState)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            state = newState;

            ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable
            {
                { STATE_KEY, (int)newState }
            };

            //ExitGames.Client.Photon.Hashtable ht = (ExitGames.Client.Photon.Hashtable)PhotonNetwork.CurrentRoom.CustomProperties.Clone();
            //ht[STATE_KEY] = (int)newState;

            PhotonNetwork.CurrentRoom.SetCustomProperties(ht);
        }

        public void SetPlayTime(float time)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable
            {
                { PLAYTIME_KEY, playTime }
            };

            //ExitGames.Client.Photon.Hashtable ht = (ExitGames.Client.Photon.Hashtable)PhotonNetwork.CurrentRoom.CustomProperties.Clone();
            //ht[PLAYTIME_KEY] = playTime;

            PhotonNetwork.CurrentRoom.SetCustomProperties(ht);
        }

        // public Methods
        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }

        public void RetryGame()
        {
            SetGameState(GameState.Retry);
        }
    }
}