using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Shurub
{
    public class GameManager : Singleton<GameManager>
    {
        protected override bool CheckDontDestroyOnLoad()
        {
            return false;
        }

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

        //void Start()
        //{
            
        //    //로비에서부터 준비 시작-> 로딩 -> 레디 -> 이후 플레잉으로 진행, 하지만 지금은 생략
        //}

        private void Update()
        {
            if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient) return;

            if (NetworkManager.Instance.CurrentRoomState == GameState.Playing)
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
            GUIManager.Instance.ResultPanel.SetActive(true);
        }

        private void CloseResultUI()
        {
            GUIManager.Instance.ResultPanel.SetActive(false);
        }

        // Public Functions

        public void OnGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Loading:
                    GameInit();
                    NetworkManager.Instance.SetGameState(GameState.Ready);
                    break;
                case GameState.Ready:
                    //카운트 다운
                    NetworkManager.Instance.SetGameState(GameState.Playing);
                    break;
                case GameState.Playing:
                    break;
                case GameState.Result:
                    OpenResultUI();
                    SetPlayerInput(false);
                    break;
                case GameState.Retry:
                    NetworkManager.Instance.SetGameState(GameState.Loading);
                    break;
            }
        }

        public void GameInit()
        {
            PlayerSpawner.Instance.ReSpawnPlayer();
            SetHP(maxHp);
            playTime = 0f;
            SetPlayTime(playTime);
            CloseResultUI();
            SetPlayerInput(true);
            PlayerManager.LocalPlayerInstance.GetComponent<PlayerController>().InitAnim();
            IngredientManager.Instance.ClearIngredient();
            TestManager.Instance.InstantiateTest();
        }

        public float GetHP()
        {
            if (!PhotonNetwork.InRoom)
            {
                return -1f;
            }

            return (float)PhotonNetwork.CurrentRoom.CustomProperties.Get(GameConstants.Network.GAME_HP_KEY, -1f);
        }

        public void SetHP(float value)
        {
            if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient) return;

            ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable
            {
                { GameConstants.Network.GAME_HP_KEY, value }
            };

            //ExitGames.Client.Photon.Hashtable ht = (ExitGames.Client.Photon.Hashtable)PhotonNetwork.CurrentRoom.CustomProperties.Clone();
            //ht[GameConstants.Network.GAME_HP_KEY] = value;

            PhotonNetwork.CurrentRoom.SetCustomProperties(ht);
        }

        public void ChangeHP(float delta)
        {
            if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient) return;

            float curHp = (float)PhotonNetwork.CurrentRoom.CustomProperties.Get(GameConstants.Network.GAME_HP_KEY);
            curHp += delta;

            ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable
            {
                { GameConstants.Network.GAME_HP_KEY, curHp }
            };

            //ExitGames.Client.Photon.Hashtable ht = (ExitGames.Client.Photon.Hashtable)PhotonNetwork.CurrentRoom.CustomProperties.Clone();
            //ht[GameConstants.Network.GAME_HP_KEY] = curHp;

            PhotonNetwork.CurrentRoom.SetCustomProperties(ht);
        }

        public void SubmitFood()
        {
            ChangeHP(HP_SUBMIT_REWARD);
        }

        public void DecreaseHP()
        {
            if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient) return;

            float hp = (float)PhotonNetwork.CurrentRoom.CustomProperties.Get(GameConstants.Network.GAME_HP_KEY);
            hp += HP_DECREASE;

            ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable
            {
                { GameConstants.Network.GAME_HP_KEY, hp }
            };

            //ExitGames.Client.Photon.Hashtable ht = (ExitGames.Client.Photon.Hashtable)PhotonNetwork.CurrentRoom.CustomProperties.Clone();
            //ht[GameConstants.Network.GAME_HP_KEY] = HP_DECREASE;

            PhotonNetwork.CurrentRoom.SetCustomProperties(ht);

            if (hp <= 0)
            {
                NetworkManager.Instance.SetGameState(GameState.Result);
            }
        }

        public void SetPlayTime(float time)
        {
            if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient) return;

            ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable
            {
                { GameConstants.Network.GAME_PLAYTIME_KEY, playTime }
            };

            //ExitGames.Client.Photon.Hashtable ht = (ExitGames.Client.Photon.Hashtable)PhotonNetwork.CurrentRoom.CustomProperties.Clone();
            //ht[GameConstants.Network.GAME_PLAYTIME_KEY] = playTime;

            PhotonNetwork.CurrentRoom.SetCustomProperties(ht);
        }

        public float GetPlayTime()
        {
            if (!PhotonNetwork.InRoom)
            {
                return -1f;
            }

            return (float)PhotonNetwork.CurrentRoom.CustomProperties.Get(GameConstants.Network.GAME_PLAYTIME_KEY, -1f);
        }

        public void GoToMain()
        {
            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene(GameConstants.Scene.MAIN_SCENE_NAME);
        }
    }
}