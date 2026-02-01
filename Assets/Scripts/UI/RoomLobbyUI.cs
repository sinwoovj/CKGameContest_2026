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

public class RoomLobbyUI : UIBase
{
    [SerializeField] private TMP_Text roomIdText;

    [SerializeField] private Toggle readyToggle;
    [SerializeField] private Button gameStartButton;

    [SerializeField] private Button maxPlayerPlusButton;
    [SerializeField] private Button maxPlayerMinusButton;
    [SerializeField] private TMP_InputField maxPlayerInput;

    [SerializeField] private Button timeLimitPlusButton;
    [SerializeField] private Button timeLimitMinusButton;
    [SerializeField] private TMP_InputField timeLimitInput;

    [SerializeField] private ToggleGroup playerListToggleGroup;
    [SerializeField] private PlayerInfoObj playerInfoPrefab;

    [SerializeField] private Button tutorialButton;

    private List<PlayerInfoObj> playerInfoObjects = new List<PlayerInfoObj>();

    private int curMaxPlayers;
    private int totalTimeLimitSeconds;

    private bool isReadyCool = false;
    private bool isBusyCool = false;

    private Dictionary<string, PlayerInfoObj.Status> playerStatusDict = new Dictionary<string, PlayerInfoObj.Status>();

    protected override void Init()
    {
        UIManager.Instance().RegisterUI(this);

        maxPlayerInput.onEndEdit.RemoveAllListeners();
        timeLimitInput.onValueChanged.RemoveAllListeners();
        timeLimitInput.onEndEdit.RemoveAllListeners();
        readyToggle.onValueChanged.RemoveAllListeners();
    }

    public override async void Show()
    {
        await Setup();
        base.Show();
    }

    private async UniTask Setup()
    {
        foreach (PlayerInfoObj player in playerInfoObjects)
        {
            player.InfoToggle.isOn = false;
        }

        maxPlayerInput.interactable = false;
        timeLimitInput.interactable = false;
        readyToggle.interactable = false;
        gameStartButton.interactable = false;

        readyToggle.gameObject.SetActive(false);
        gameStartButton.gameObject.SetActive(false);

        await UniTask.WaitUntil(() => PhotonNetwork.InRoom, cancellationToken: this.GetCancellationTokenOnDestroy());

        if (PhotonNetwork.IsMasterClient)
        {
            gameStartButton.gameObject.SetActive(true);

            maxPlayerInput.interactable = true;
            timeLimitInput.interactable = true;

            maxPlayerInput.onEndEdit.AddListener(OnEndEditMaxPlayerInput);

            timeLimitInput.onValidateInput += ValidateTimeLimitChar;
            timeLimitInput.onValueChanged.AddListener(OnValueChangedTimeLimitInput);
            timeLimitInput.onEndEdit.AddListener(OnEndEditTimeLimitInput);
        }
        else
        {
            readyToggle.gameObject.SetActive(true);

            readyToggle.isOn = false;
            readyToggle.interactable = true;

            readyToggle.onValueChanged.AddListener(OnValueChangedReadyToggle);
        }

        roomIdText.text = PhotonNetwork.CurrentRoom.Name;

        curMaxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
        maxPlayerInput.text = curMaxPlayers.ToString();

        totalTimeLimitSeconds = (int)PhotonNetwork.CurrentRoom.CustomProperties[GameConstants.Network.ROOM_TIME_LIMIT_HASH_PROP];
        int min = totalTimeLimitSeconds / 60;
        int sec = totalTimeLimitSeconds % 60;
        timeLimitInput.text = $"{min:00} : {sec:00}";
    }

    public override void Hide(Action onConfirmed, bool force)
    {
        if (force)
        {
            base.Hide(onConfirmed, force);
        }
        else if (PhotonNetwork.InRoom)
        {
            ModalManager.Instance().OpenNewModal("경고", "방을 나가시겠습니까?", yesAction: () =>
            {
                PhotonNetwork.LeaveRoom();
                base.Hide(onConfirmed, force);
            });
        }
        else
        {
            base.Hide(onConfirmed, force);
        }
    }

    private void Update()
    {
        if (!PhotonNetwork.InRoom)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.F5) && !UIManager.Instance().GetUI<TutorialUI>().IsOpenned())
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (CheckGameStartable())
                {
                    OnClickGameStartButton();
                }
            }
            else if (readyToggle.interactable)
            {
                readyToggle.isOn = !readyToggle.isOn;
            }
        }

        UpdateReadyToggle();
        UpdateTutorialButton();
        UpdateGameStartButton();
        UpdateMaxPlayers();
    }

    private void OnClickGameStartButton()
    {
        // Todo: 게임 시작
        Debug.Log("게임 시작!");

        // #Critical
        // Load the Room Level.
        PhotonNetwork.MasterClient.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "pNum", 0 } });
        PhotonNetwork.LoadLevel("InGame");
    }

    private bool CheckGameStartable()
    {
        if (PhotonNetwork.IsMasterClient && playerStatusDict.Count == PhotonNetwork.CurrentRoom.MaxPlayers && playerStatusDict.Values.ToList().TrueForAll((s) => s == PlayerInfoObj.Status.Ready))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void UpdateGameStartButton()
    {
        if (CheckGameStartable())
        {
            gameStartButton.interactable = true;
        }
        else
        {
            gameStartButton.interactable = false;
        }
    }

    private void UpdateReadyToggle()
    {
        readyToggle.interactable = !isReadyCool;
    }

    private void UpdateTutorialButton()
    {
        tutorialButton.interactable = !isBusyCool;
    }

    private void OnClickMaxPlayerPlusButton()
    {
        if (curMaxPlayers >= GameConstants.Network.MAX_PLAYERS_PER_ROOM)
        {
            return;
        }

        curMaxPlayers++;
    }

    private void OnClickMaxPlayerMinusButton()
    {
        if (curMaxPlayers <= GameConstants.Network.MIN_PLAYERS_PER_ROOM)
        {
            return;
        }

        curMaxPlayers--;
    }

    private void OnEndEditMaxPlayerInput(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            maxPlayerInput.text = curMaxPlayers.ToString();
            return;
        }

        if (!int.TryParse(value, out int number))
        {
            maxPlayerInput.text = curMaxPlayers.ToString();
            return;
        }

        curMaxPlayers = Mathf.Clamp(number, GameConstants.Network.MIN_PLAYERS_PER_ROOM, GameConstants.Network.MAX_PLAYERS_PER_ROOM);
    }

    private void UpdateMaxPlayers()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.MaxPlayers = curMaxPlayers;
        }
        else
        {
            curMaxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
        }

        maxPlayerInput.text = curMaxPlayers.ToString();
    }

    private char ValidateTimeLimitChar(string text, int charIndex, char addedChar)
    {
        if (char.IsDigit(addedChar))
        {
            return addedChar;
        }

        return '\0';
    }

    private void OnValueChangedTimeLimitInput(string value)
    {
    }

    private void OnEndEditTimeLimitInput(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            totalTimeLimitSeconds = 0;
            UpdateTimeLimit();
            return;
        }

        string digits = "";
        foreach (char c in value)
        {
            if (char.IsDigit(c))
            {
                digits += c;
            }
        }

        digits = digits.PadRight(4, '0')[..4];
        value = digits.Insert(2, ":");

        if (!value.TryParseTime(out totalTimeLimitSeconds))
        {
            totalTimeLimitSeconds = 0;
        }

        UpdateTimeLimit();
    }

    private void OnClickTimeLimitPlusButton()
    {
        totalTimeLimitSeconds++;
        totalTimeLimitSeconds = Mathf.Clamp(totalTimeLimitSeconds, 0, 59 * 60 + 59);
        UpdateTimeLimit();
    }

    private void OnClickTimeLimitMinusButton()
    {
        totalTimeLimitSeconds--;
        totalTimeLimitSeconds = Mathf.Clamp(totalTimeLimitSeconds, 0, 59 * 60 + 59);
        UpdateTimeLimit();
    }

    private void UpdateTimeLimit()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable
        {
            { GameConstants.Network.ROOM_TIME_LIMIT_HASH_PROP, totalTimeLimitSeconds }
        });
    }

    public void OnUpdatedCustomProperties(Hashtable propertiesThatChanged)
    {
        if (!IsOpenned())
        {
            return;
        }

        if (propertiesThatChanged.TryGetValue(GameConstants.Network.ROOM_TIME_LIMIT_HASH_PROP, out object value))
        {
            totalTimeLimitSeconds = (int)value;

            int min = totalTimeLimitSeconds / 60;
            int sec = totalTimeLimitSeconds % 60;
            timeLimitInput.text = $"{min:00} : {sec:00}";
        }
    }

    public void OnUpdatedPlayerList()
    {
        if (!IsOpenned())
        {
            return;
        }

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

            if (player.CustomProperties.ContainsKey(GameConstants.Network.PLAYER_STATUS_HASH_PROP))
            {
                playerStatusDict[player.UserId] = (PlayerInfoObj.Status)(int)player.CustomProperties[GameConstants.Network.PLAYER_STATUS_HASH_PROP];
            }
        }
    }

    public void OnChangedMaster(Photon.Realtime.Player newMaster)
    {
        if (!IsOpenned())
        {
            return;
        }

        if (PhotonNetwork.LocalPlayer == newMaster)
        {
            NetworkManager.Instance().SetLobbyPlayerStatus(PhotonNetwork.LocalPlayer, PlayerInfoObj.Status.Ready);
            //playerInfoObjects.Find(p => p.Info == PhotonNetwork.LocalPlayer).Init(PhotonNetwork.LocalPlayer);
            Setup().Forget();
        }
    }

    private void OnValueChangedReadyToggle(bool isOn)
    {
        NetworkManager.Instance().SetLobbyPlayerStatus(PhotonNetwork.LocalPlayer, isOn ? PlayerInfoObj.Status.Ready : PlayerInfoObj.Status.NotReady);
        CalcReadyCool().Forget();

        readyToggle.transform.GetChild(0).gameObject.SetActive(!isOn);
        readyToggle.targetGraphic.color = isOn ? readyToggle.colors.selectedColor : readyToggle.colors.normalColor;
    }

    private async UniTaskVoid CalcReadyCool()
    {
        isReadyCool = true;
        await UniTask.Delay(GameConstants.Network.LOBBY_READY_COOL_TIME, cancellationToken: this.GetCancellationTokenOnDestroy());
        isReadyCool = false;
    }

    private void OnClickTutorialButton()
    {
        CalcBusyCool().Forget();
        NetworkManager.Instance().SetLobbyPlayerStatus(PhotonNetwork.LocalPlayer, PlayerInfoObj.Status.Busy);
        UIManager.Instance().ShowUI<TutorialUI>(hidePrev: false);
    }

    private async UniTaskVoid CalcBusyCool()
    {
        isBusyCool = true;
        await UniTask.Delay(GameConstants.Network.LOBBY_BUSY_COOL_TIME, cancellationToken: this.GetCancellationTokenOnDestroy());
        isBusyCool = false;
    }
}
