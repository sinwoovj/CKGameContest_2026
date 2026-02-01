using ExitGames.Client.Photon;
using JJM;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NetworkManager : Singleton<NetworkManager>
{
    public bool IsConnecting { get; private set; }

    private Dictionary<string, RoomInfo> availableRooms = new Dictionary<string, RoomInfo>();

    protected override void OnAwake()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.IsMessageQueueRunning = true;
        PhotonNetwork.GameVersion = Application.version;
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

        UIManager.Instance().GetUI<JoinRoomUI>().OnUpdatedRoomList(availableRooms.Values.ToList());
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

        ModalManager.Instance().OpenNewModal("방 생성 실패", msg, disableNo: true, yesAction: () => UIManager.Instance().ReturnPrevUI(force: true));
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

        ModalManager.Instance().OpenNewModal("입장 실패", msg, disableNo: true, yesAction: () => UIManager.Instance().ReturnPrevUI(force: true));
        Debug.LogFormat("방 입장 실패. Code: {0}, Message: {1}", returnCode, message);
    }

    public override void OnJoinedRoom()
    {
        int status = PhotonNetwork.IsMasterClient ? (int)PlayerInfoObj.Status.Ready : (int)PlayerInfoObj.Status.NotReady;
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
        {
            { GameConstants.Network.PLAYER_STATUS_HASH_PROP, status }
        });

        UIManager.Instance().ShowUI<RoomLobbyUI>();
        Debug.Log("방 입장 완료.");
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable());

        Debug.Log("방 퇴장 완료.");
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        UIManager.Instance().GetUI<RoomLobbyUI>().OnUpdatedCustomProperties(propertiesThatChanged);
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }

        UIManager.Instance().GetUI<RoomLobbyUI>().OnUpdatedPlayerList();
        Debug.LogFormat("플레이어가 입장함. Id: {0}, 플레이어 수: {1}", newPlayer.UserId, PhotonNetwork.PlayerList.Length);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount < PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            PhotonNetwork.CurrentRoom.IsOpen = true;
            PhotonNetwork.CurrentRoom.IsVisible = true;
        }

        UIManager.Instance().GetUI<RoomLobbyUI>().OnUpdatedPlayerList();
        Debug.LogFormat("플레이어가 퇴장함. Id: {0}, 플레이어 수: {1}", otherPlayer.UserId, PhotonNetwork.PlayerList.Length);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        UIManager.Instance().GetUI<RoomLobbyUI>().OnChangedMaster(newMasterClient);
        Debug.LogFormat("마스터 클라이언트가 변경됨. Id: {0}", newMasterClient.UserId);
    }

    public void KickPlayer(Player target)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        Hashtable props = new Hashtable
        {
            { GameConstants.Network.PLAYER_KICK_HASH_PROP, true }
        };

        target.SetCustomProperties(props);
    }

    public void SetLobbyPlayerStatus(Player target, PlayerInfoObj.Status status)
    {
        if (!PhotonNetwork.InRoom || !PhotonNetwork.PlayerList.Contains(target))
        {
            return;
        }

        target.SetCustomProperties(new Hashtable
        {
            { GameConstants.Network.PLAYER_STATUS_HASH_PROP, (int)status }
        });
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (PhotonNetwork.InRoom)
        {
            //if (changedProps.ContainsKey(GameConstants.Network.PLAYER_STATUS_HASH_PROP))
            //{
            //    UIManager.Instance().GetUI<RoomLobbyUI>().OnUpdatedPlayerList();
            //}

            if (targetPlayer == PhotonNetwork.LocalPlayer && changedProps.ContainsKey(GameConstants.Network.PLAYER_KICK_HASH_PROP))
            {
                PhotonNetwork.LeaveRoom();
                ModalManager.Instance().OpenNewModal("", "방에서 강제퇴장 되었습니다.", disableNo: true);

                if (UIManager.Instance().GetUI<TutorialUI>().IsOpenned())
                {
                    UIManager.Instance().HideUI<TutorialUI>(force: true);
                }
                UIManager.Instance().ShowUI<PlayUI>(removePrev: true, force: true);
            }

            UIManager.Instance().GetUI<RoomLobbyUI>().OnUpdatedPlayerList();
        }
    }
}
