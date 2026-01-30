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
        public GameState gameState = GameState.None;

        void Start()
        {
            Instance = this;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) UIManager.Instance.SetPausePanel();
        }

        // Called when the local player left the room.
        // We need to load the launcher scene.
        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(0); // Main Scene
            PhotonNetwork.LocalPlayer.TagObject = null;
        }

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }

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
    }
}