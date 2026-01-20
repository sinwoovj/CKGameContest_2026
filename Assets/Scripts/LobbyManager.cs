using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : Singleton<LobbyManager>
{
    [SerializeField] private GameObject _roomListObj;
    [SerializeField] private GameObject _roomObjPrefab;

    private List<GameObject> _roomObjs = new List<GameObject>();

    public int MaxPlayers { get; set; } = 20;

    private Dictionary<string, RoomInfo> _availableRooms = new Dictionary<string, RoomInfo>();

    public override void OnJoinedLobby()
    {
        Debug.Log("로비 접속 성공.");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        for (int i = 0; i < _roomListObj.transform.childCount; i++)
        {
            Destroy(_roomListObj.transform.GetChild(i).gameObject);
        }

        foreach (RoomInfo room in roomList)
        {
            // 방이 제거된 경우 리스트에서 제외
            if (room.RemovedFromList)
            {
                _availableRooms.Remove(room.Name);
            }
            else if (room.PlayerCount < room.MaxPlayers)
            {
                _availableRooms[room.Name] = room;

                GameObject roomObj = Instantiate(_roomObjPrefab, _roomListObj.transform);
                roomObj.GetComponent<Button>().onClick.AddListener(() => PhotonNetwork.JoinRoom(room.Name));
                roomObj.GetComponentInChildren<TMP_Text>().SetText($"{room.Name} ({room.PlayerCount} / {room.MaxPlayers})");
            }
        }

        Debug.Log($"입장 가능한 방 개수: {_availableRooms.Count}");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        if (_availableRooms.Count != 0)
        {
            Debug.Log("랜덤 방 입장에 실패함. 재시도...");
            PhotonNetwork.JoinRandomRoom(_availableRooms.GetRandomValue().CustomProperties, 0);
            return;
        }

        Debug.Log("입장 가능한 방이 없음. 새로운 방 생성...");
        CreateDefaultRoom();
    }

    public override void OnJoinedRoom()
    {
        Debug.LogFormat("방 입장 완료. PlayerId: {0}", PhotonNetwork.LocalPlayer.UserId);
    }

    public override void OnLeftRoom()
    {
        Debug.Log("방 퇴장 완료.");
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.LogFormat("플레이어가 입장함. Id: {0}, 플레이어 수: {1}", newPlayer.UserId, PhotonNetwork.PlayerList.Length);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.LogFormat("플레이어가 퇴장함. Id: {0}, 플레이어 수: {1}", otherPlayer.UserId, PhotonNetwork.PlayerList.Length);
    }

    public void JoinLobby()
    {
        PhotonNetwork.JoinLobby();
    }

    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public void CreateRoom(string name, RoomOptions options)
    {
        PhotonNetwork.CreateRoom(name, options);
    }

    public void CreateDefaultRoom()
    {
        string name = Guid.NewGuid().ToString()[..4];

        RoomOptions options = new RoomOptions();
        options.IsOpen = true;
        options.IsVisible = true;
        options.PublishUserId = true;
        options.MaxPlayers = MaxPlayers;
        //options.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable()
        //{
        //    { "RoomNumber", Random.Range(0, 100000) }
        //};
        //options.CustomRoomPropertiesForLobby = new string[] { "RoomNumber" };

        PhotonNetwork.CreateRoom(name, options);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
}
