using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shurub
{
    public class NetworkManager : SingletonPun<NetworkManager>
    {
        public bool IsConnecting { get; private set; }

        public GameState CurrentRoomState { get; private set; }
        public GameDifficulty CurrentRoomDifficulty { get; private set; }

        private Dictionary<string, RoomInfo> availableRooms = new Dictionary<string, RoomInfo>();

        protected override void OnAwake()
        {
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.IsMessageQueueRunning = true;
            PhotonNetwork.GameVersion = Application.version;
            PhotonNetwork.SendRate = 60;
            PhotonNetwork.SerializationRate = 60;
            PhotonNetwork.NickName = Guid.NewGuid().ToString()[..4];

            Connect();
        }

        public override void OnConnectedToMaster()
        {
            IsConnecting = false;
            PhotonNetwork.JoinLobby();

            Debug.Log("클라이언트가 마스터에 연결됨.");
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogWarningFormat("서버와의 연결이 끊어짐. 사유: {0}", cause);
        }

        public void Connect()
        {
            if (IsConnecting)
            {
                return;
            }

            IsConnecting = true;
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnJoinedLobby()
        {
            Debug.Log("로비 입장 완료.");
        }

        public override void OnLeftLobby()
        {
            Debug.Log("로비 퇴장 완료.");
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            foreach (RoomInfo room in roomList)
            {
                // 방이 제거된 경우 리스트에서 제외
                if (room.RemovedFromList)
                {
                    availableRooms.Remove(room.Name);
                }
                else
                {
                    availableRooms[room.Name] = room;
                }
            }

            UIManager.Instance.GetUI<JoinRoomUI>().OnUpdatedRoomList(availableRooms.Values.ToList());
            Debug.Log($"입장 가능한 방 개수: {availableRooms.Count}");
        }

        //public override void OnJoinRandomFailed(short returnCode, string message)
        //{
        //    if (availableRooms.Count != 0)
        //    {
        //        Debug.Log("랜덤 방 입장에 실패함. 재시도...");
        //        PhotonNetwork.JoinRandomRoom(availableRooms.GetRandomValue().CustomProperties, 0);
        //        return;
        //    }

        //    Debug.Log("입장 가능한 방이 없음. 새로운 방 생성...");
        //    CreateDefaultRoom();
        //}

        public override void OnCreatedRoom()
        {
            Debug.Log("방 생성 완료.");
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            string msg = returnCode switch
            {
                ErrorCode.InvalidAuthentication => "인증에 실패했습니다. 게임을 재시작 한 후 다시 시도해주세요.",
                ErrorCode.InvalidOperation => "잘못된 요청입니다.",
                ErrorCode.InternalServerError => "서버에 오류가 발생했습니다. 잠시 후 다시 시도해주세요.",
                ErrorCode.UserBlocked => "차단된 유저입니다.",
                _ => "알 수 없는 오류"
            };

            ModalManager.Instance.OpenNewModal("방 생성 실패", msg, disableNo: true, yesAction: () => UIManager.Instance.ReturnPrevUI(force: true));
            Debug.LogFormat("방 생성 실패. Code: {0}, Message: {1}", returnCode, message);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            string msg = returnCode switch
            {
                ErrorCode.GameFull => "방이 가득 찼습니다.",
                ErrorCode.GameClosed => "방이 닫혔습니다.",
                ErrorCode.GameDoesNotExist => "방이 없습니다.",
                ErrorCode.InvalidAuthentication => "인증에 실패했습니다. 게임을 재시작 한 후 다시 시도해주세요.",
                ErrorCode.InvalidOperation => "잘못된 요청입니다.",
                ErrorCode.InternalServerError => "서버에 오류가 발생했습니다. 잠시 후 다시 시도해주세요.",
                ErrorCode.UserBlocked => "차단된 유저입니다.",
                _ => "알 수 없는 오류"
            };

            ModalManager.Instance.OpenNewModal("입장 실패", msg, disableNo: true, yesAction: () => UIManager.Instance.ReturnPrevUI(force: true));
            Debug.LogFormat("방 입장 실패. Code: {0}, Message: {1}", returnCode, message);
        }

        public override void OnJoinedRoom()
        {
            int status = PhotonNetwork.IsMasterClient ? (int)PlayerInfoObj.Status.Ready : (int)PlayerInfoObj.Status.NotReady;
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
            {
                { GameConstants.Network.PLAYER_STATUS_KEY, status }
            });

            UIManager.Instance.ShowUI<RoomLobbyUI>();
            Debug.Log("방 입장 완료.");
        }

        public override void OnLeftRoom()
        {
            PhotonNetwork.LocalPlayer.TagObject = null;
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable());

            Debug.Log("방 퇴장 완료.");
        }

        public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable properties)
        {
            if (properties.TryGetValue(GameConstants.Network.GAME_STATE_KEY, out object state))
            {
                GameState newState = (GameState)(int)state;
                if (CurrentRoomState != newState)
                {
                    CurrentRoomState = newState;
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.OnGameStateChanged(CurrentRoomState);
                    }
                }

                if (newState == GameState.Boot)
                {
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    PhotonNetwork.CurrentRoom.IsVisible = false;

                    UIManager.Instance.HideUI<RoomLobbyUI>(force: true);
                    PhotonNetwork.LoadLevel("InGame");
                    SetGameState(GameState.Loading);
                }

                if (newState == GameState.Lobby)
                {
                    PhotonNetwork.CurrentRoom.IsOpen = true;
                    PhotonNetwork.CurrentRoom.IsVisible = true;
                }
            }

            if (properties.TryGetValue(GameConstants.Network.GAME_DIFFICULTY_KEY, out object difficulty))
            {
                CurrentRoomDifficulty = (GameDifficulty)(int)difficulty;
            }

            if (properties.TryGetValue(GameConstants.Network.GAME_HP_KEY, out object hp))
            {
                GameManager.Instance.currentHp = (float)hp;
                GUIManager.Instance.UpdateHPUI();
            }

            if (properties.TryGetValue(GameConstants.Network.GAME_PLAYTIME_KEY, out object time))
            {
                GUIManager.Instance.UpdateTimeUI();
            }

            UIManager.Instance.GetUI<RoomLobbyUI>().OnUpdatedCustomProperties(properties);
        }

        public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                ReassignPlayerNumbers();
                if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
                {
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    PhotonNetwork.CurrentRoom.IsVisible = false;
                }
            }

            UIManager.Instance.GetUI<RoomLobbyUI>().OnUpdatedPlayerList();
            Debug.LogFormat("플레이어가 입장함. Id: {0}, 플레이어 수: {1}", newPlayer.UserId, PhotonNetwork.PlayerList.Length);
        }

        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            if (PhotonNetwork.NetworkClientState == ClientState.Leaving)
            {
                return;
            }

            if (PhotonNetwork.InRoom)
            {
                if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount < PhotonNetwork.CurrentRoom.MaxPlayers)
                {
                    PhotonNetwork.CurrentRoom.IsOpen = true;
                    PhotonNetwork.CurrentRoom.IsVisible = true;
                }

                UIManager.Instance.GetUI<RoomLobbyUI>().OnUpdatedPlayerList();
            }

            Debug.LogFormat("플레이어가 퇴장함. Id: {0}, 플레이어 수: {1}", otherPlayer.UserId, PhotonNetwork.PlayerList.Length);
        }

        public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
        {
            ReassignPlayerNumbers();
            UIManager.Instance.GetUI<RoomLobbyUI>().OnChangedMaster(newMasterClient);
            Debug.LogFormat("마스터 클라이언트가 변경됨. Id: {0}", newMasterClient.UserId);
        }

        public void SetGameState(GameState newState)
        {
            if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient) return;

            ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable
            {
                { GameConstants.Network.GAME_STATE_KEY, (int)newState }
            };

            //ExitGames.Client.Photon.Hashtable ht = (ExitGames.Client.Photon.Hashtable)PhotonNetwork.CurrentRoom.CustomProperties.Clone();
            //ht[GameConstants.Network.GAME_STATE_KEY] = (int)newState;

            PhotonNetwork.CurrentRoom.SetCustomProperties(ht);
        }

        public void KickPlayer(Photon.Realtime.Player target)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            target.SetCustomProperties(new Hashtable
            {
                { GameConstants.Network.PLAYER_KICK_KEY, true }
            });
        }

        public void SetLobbyPlayerStatus(Photon.Realtime.Player target, PlayerInfoObj.Status status)
        {
            if (!PhotonNetwork.InRoom || !PhotonNetwork.PlayerList.Contains(target))
            {
                return;
            }

            target.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
        {
            { GameConstants.Network.PLAYER_STATUS_KEY, (int)status }
        });
        }

        public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
        {
            if (PhotonNetwork.InRoom)
            {
                //if (changedProps.ContainsKey(GameConstants.Network.PLAYER_STATUS_HASH_PROP))
                //{
                //    UIManager.Instance.GetUI<RoomLobbyUI>().OnUpdatedPlayerList();
                //}

                if (targetPlayer == PhotonNetwork.LocalPlayer)
                {
                    if (PlayerManager.LocalPlayerInstance != null && PlayerSpawner.Instance != null)
                    {
                        int myPNum;
                        if (changedProps.TryGetValue(GameConstants.Network.PLAYER_NUMBER_KEY, out object value))
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

                    if (changedProps.ContainsKey(GameConstants.Network.PLAYER_KICK_KEY))
                    {
                        PhotonNetwork.LeaveRoom();
                        ModalManager.Instance.OpenNewModal("", "방에서 강제퇴장 되었습니다.", disableNo: true);

                        if (UIManager.Instance.GetUI<TutorialUI>().IsOpenned())
                        {
                            UIManager.Instance.HideUI<TutorialUI>(force: true);
                        }
                        UIManager.Instance.ShowUI<PlayUI>(removePrev: true, force: true);
                    }
                }

                UIManager.Instance.GetUI<RoomLobbyUI>().OnUpdatedPlayerList();
            }
        }

        public void ReassignPlayerNumbers()
        {
            List<Photon.Realtime.Player> players = PhotonNetwork.PlayerList.ToList();

            // MasterClient
            Photon.Realtime.Player master = PhotonNetwork.MasterClient;

            ExitGames.Client.Photon.Hashtable masterProp = new ExitGames.Client.Photon.Hashtable { { "pNum", 0 } };

            //ExitGames.Client.Photon.Hashtable masterProp = (ExitGames.Client.Photon.Hashtable)master.CustomProperties.Clone();
            //masterProp[GameConstants.Network.PLAYER_NUMBER_HASH_PROP] = 0;
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
                //ExitGames.Client.Photon.Hashtable prop = (ExitGames.Client.Photon.Hashtable)player.CustomProperties.Clone();
                //prop[GameConstants.Network.PLAYER_NUMBER_HASH_PROP] = pNum++;
                player.SetCustomProperties(prop);
            }
        }
    }
}

