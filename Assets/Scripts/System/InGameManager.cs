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
    public class InGameManager : Singleton<InGameManager>
    {
        protected override bool CheckDontDestroyOnLoad()
        {
            return false;
        }

        private List<Player> players = new List<Player>();

        private Player localPlayer = null;
        public Player LocalPlayer
        {
            get
            {
                if (localPlayer == null)
                {
                    localPlayer = GetLocalPlayer();
                }

                return localPlayer;
            }
        }

        // Room Properties
        public float maxHp = 100f;
        public float currentHp = 0f;
        public float playTime = 0f;

        // Logic of Hp Decrease
        float hpTickTimer;
        const float HP_TICK_INTERVAL = 2f; // per 2s
        const int HP_DECREASE = -1;

        // Monohaviour Functions

        void Start()
        {
            //로비에서부터 준비 시작-> 로딩 -> 레디 -> 이후 플레잉으로 진행, 하지만 지금은 생략
            NetworkManager.Instance.SetGameState(GameState.Loading);
        }

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

        public List<Player> GetAllPlayers() => players;

        public void RegisterPlayer(Player newPlayer)
        {
            if (players.Contains(newPlayer))
            {
                return;
            }

            players.Add(newPlayer);
        }

        public void UnregisterPlayer(Player removedPlayer)
        {
            players.Remove(removedPlayer);
        }

        public Player GetLocalPlayer()
        {
            Player local = null;
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].GetComponent<PlayerManager>().photonView.IsMine)
                {
                    local = players[i];
                    break;
                }
            }

            return local;
        }

        // Private Functions

        private void SetPlayerInput(bool val)
        {
            LocalPlayer.GetComponent<PlayerInput>().enabled = val;
        }

        private void OpenResultUI()
        {
            GUIManager.Instance.UpdateTotalTimeUI();

            UIManager.Instance.ClearAllUIs();
            UIManager.Instance.ShowUI<GameResultUI>();
            //GUIManager.Instance.ResultPanel.SetActive(true);
        }

        //private void CloseResultUI()
        //{
        //    UIManager.Instance.HideUI<GameResultUI>();
        //    //GUIManager.Instance.ResultPanel.SetActive(false);
        //}

        // Public Functions

        public void OnGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Loading:
                    GameInit();
                    SetPlayerInput(false);
                    UIManager.Instance.ClearAllUIs();
                    NetworkManager.Instance.SetGameState(GameState.Ready);
                    SoundManager.Instance.Play("InGameBGM");
                    break;
                case GameState.Ready:
                    //카운트 다운
                    GUIManager.Instance.CountDownUI();
                    break;
                case GameState.Playing:
                    SetPlayerInput(true);
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
        void GameInit()
        {
            PlayerSpawner.Instance.ReSpawnPlayer();
            SetHP(maxHp);
            playTime = 0f;
            SetPlayTime(playTime);
            LocalPlayer.GetComponent<Animator>().SetTrigger("Default");
            IngredientManager.Instance.ClearIngredient();
            TestManager.Instance.InstantiateTest();
            PlayerUIManager.Instance.ClearPlateUI();
            OrderUIManager.Instance.ClearOrder();
            OrderManager.Instance.Init();
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
    }
}