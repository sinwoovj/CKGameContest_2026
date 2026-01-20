using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : Singleton<NetworkManager>
{
    public bool IsConnecting { get; private set; }

    protected override void OnAwake()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.IsMessageQueueRunning = true;
        PhotonNetwork.GameVersion = Application.version;
    }

    public override void OnConnectedToMaster()
    {
        IsConnecting = false;
        Debug.Log("클라이언트가 마스터에 연결됨.");
        LobbyManager.Instance().JoinLobby();
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

    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
    }
}
