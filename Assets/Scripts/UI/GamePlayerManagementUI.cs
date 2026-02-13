using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using ExitGames.Client.Photon;
using Shurub;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class GamePlayerManagementUI : UIBase<GamePlayerManagementUI>
{
    [SerializeField] private ToggleGroup playerListToggleGroup;
    [SerializeField] private PlayerInfoObj playerInfoPrefab;

    private List<PlayerInfoObj> playerInfoObjects = new List<PlayerInfoObj>();

    private Dictionary<string, PlayerStatus> playerStatusDict = new Dictionary<string, PlayerStatus>();

    public void OnUpdatedPlayerList()
    {
        playerStatusDict.Clear();
        Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < playerInfoObjects.Count; i++)
        {
            playerInfoObjects[i].gameObject.SetActive(false);
        }

        for (int i = playerInfoObjects.Count; i < players.Length; i++)
        {
            PlayerInfoObj playerObj = Instantiate(playerInfoPrefab, playerListToggleGroup.transform);
            playerObj.InfoToggle.group = playerListToggleGroup;
            playerObj.gameObject.SetActive(false);
            playerInfoObjects.Add(playerObj);
        }

        for (int i = 0; i < players.Length; i++)
        {
            Photon.Realtime.Player player = players[i];
            PlayerInfoObj playerObj = playerInfoObjects[i];
            playerObj.gameObject.SetActive(true);
            playerObj.Init(player);

            if (player.CustomProperties.ContainsKey(GameConstants.Network.PLAYER_STATUS_KEY))
            {
                playerStatusDict[player.UserId] = (PlayerStatus)(int)player.CustomProperties[GameConstants.Network.PLAYER_STATUS_KEY];
            }
        }
    }
}
