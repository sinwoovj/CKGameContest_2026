using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
            Matching,      // 매칭 중
            Loading,       // 씬/리소스 로딩
            Ready,         // 카운트다운
            Playing,       // 실제 플레이
            Paused,        // 일시정지
            Result,        // 결과 화면
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
        const float HP_TICK_INTERVAL = 1f; // per 1s
        const int HP_DECREASE = -1;

        // Monohaviour Functions
        private void Awake()
        {
            Instance = this;
        }
        void Start()
        {
            SetGameState(GameState.Boot);
            //로비에서부터 준비 시작-> 로딩 -> 레디 -> 이후 플레잉으로 진행, 하지만 지금은 생략
            SetHP(maxHp);
            SetGameState(GameState.Playing);
        }

        private void Update()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            if(state == GameState.Playing)
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

        // Called when the local player left the room.
        // We need to load the launcher scene.
        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(0); // Main Scene
            PhotonNetwork.LocalPlayer.TagObject = null;
        }

        // Photon Callback Functions
        public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
        {
            ReassignPlayerNumbers();
        }

        public override void OnPlayerEnteredRoom(Photon.Realtime.Player other)
        {
            Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom

                ReassignPlayerNumbers();
            }
        }

        public override void OnPlayerLeftRoom(Photon.Realtime.Player other)
        {
            Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom

                ReassignPlayerNumbers();
            }
        }
        public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable properties)
        {
            if (properties.TryGetValue(HP_KEY, out object hp))
            {
                currentHp = (float)hp;
                UIManager.Instance.UpdateHPUI();
            }

            if (properties.TryGetValue(PLAYTIME_KEY, out object time))
            {
                UIManager.Instance.UpdateTimeUI();
            }

            if (properties.TryGetValue(STATE_KEY, out object _state))
            {
                state = (GameState)(int)_state;
                OnGameStateChanged(state);
            }
        }

        public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
        {
            int myPNum;
            if (targetPlayer == PhotonNetwork.LocalPlayer &&
                changedProps.TryGetValue("pNum", out object value))
            {
                myPNum = (int)value;
                Debug.Log("MyPNum updated: " + myPNum);
                if (!PlayerManager.LocalPlayerInstance.GetComponent<Player>().isSpawned)
                {
                    PlayerSpawner.Instance.ReSpawnPlayer();
                    PlayerManager.LocalPlayerInstance.GetComponent<Player>().isSpawned = true;
                }
            }
        }

        // Functions

        void ReassignPlayerNumbers()
        {
             List<Photon.Realtime.Player> players = PhotonNetwork.PlayerList.ToList();

            // MasterClient
            Photon.Realtime.Player master = PhotonNetwork.MasterClient;
            ExitGames.Client.Photon.Hashtable masterProp = new ExitGames.Client.Photon.Hashtable { { "pNum", 0 } };
            master.SetCustomProperties(masterProp);

            // Extra Player
            List<Photon.Realtime.Player> others = players.Where(p => p != master).ToList();

            // Mix Random
            for (int i = 0; i < others.Count; i++)
            {
                int rand = UnityEngine.Random.Range(i, others.Count);
                (others[i], others[rand]) = (others[rand], others[i]);
            }

            // if pNum 1~
            int pNum = 1;
            foreach (var player in others)
            {
                ExitGames.Client.Photon.Hashtable prop = new ExitGames.Client.Photon.Hashtable { { "pNum", pNum++ } };
                player.SetCustomProperties(prop);
            }
        }

        void OnGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Result:
                    //ShowResultUI();
                    //DisablePlayerInput();
                    break;
            }
        }

        void SetHP(float value)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable
            {
                { HP_KEY, value }
            };

            PhotonNetwork.CurrentRoom.SetCustomProperties(ht);
        }

        void ChangeHP(float delta)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            float currentHP = (float)PhotonNetwork.CurrentRoom.CustomProperties[HP_KEY];
            currentHP += delta;

            ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable
            {
                { HP_KEY, currentHP }
            };

            PhotonNetwork.CurrentRoom.SetCustomProperties(ht);
        }

        void DecreaseHP()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            float hp = (float)PhotonNetwork.CurrentRoom.CustomProperties[HP_KEY];
            hp += HP_DECREASE;

            ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable
            {
                { HP_KEY, hp }
            };

            PhotonNetwork.CurrentRoom.SetCustomProperties(ht);

            if (hp <= 0)
            {
                SetGameState(GameState.Result);
            }
        }

        void SetGameState(GameState newState)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            state = newState;

            ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable
            {
                { STATE_KEY, (int)newState }
            };

            PhotonNetwork.CurrentRoom.SetCustomProperties(ht);
        }
        
        void SetPlayTime(float time)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable
            {
                { PLAYTIME_KEY, playTime }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(ht);
        }

        // Methods
        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }

    }
}