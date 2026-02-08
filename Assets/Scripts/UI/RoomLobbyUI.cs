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

public class RoomLobbyUI : UIBase
{
    [SerializeField] private TMP_Text roomNameText;

    [SerializeField] private Toggle readyToggle;
    [SerializeField] private Button gameStartButton;

    [SerializeField] private Button maxPlayerPlusButton;
    [SerializeField] private Button maxPlayerMinusButton;
    [SerializeField] private TMP_InputField maxPlayerInput;

    //[SerializeField] private Button timeLimitPlusButton;
    //[SerializeField] private Button timeLimitMinusButton;
    //[SerializeField] private TMP_InputField timeLimitInput;

    [SerializeField] private Toggle privateCheckToggle;
    [SerializeField] private TMP_InputField passwordInput;

    [SerializeField] private Button difficultyPlusButton;
    [SerializeField] private Button difficultyMinusButton;
    [SerializeField] private TMP_InputField difficultyInput;

    [SerializeField] private ToggleGroup playerListToggleGroup;
    [SerializeField] private PlayerInfoObj playerInfoPrefab;

    [SerializeField] private Button tutorialButton;

    private List<PlayerInfoObj> playerInfoObjects = new List<PlayerInfoObj>();

    private int curMaxPlayers;
    private bool isPrivate;
    private string password = "";
    private GameDifficulty curDifficulty;
    //private int totalTimeLimitSeconds;

    private bool isReadyCool = false;
    private bool isBusyCool = false;

    private Dictionary<string, PlayerInfoObj.Status> playerStatusDict = new Dictionary<string, PlayerInfoObj.Status>();

    public override bool NeedConfirmWhenHide => true;
    protected override string ConfirmTitle => "경고";
    protected override string ConfirmMessage => "방을 나가시겠습니까?";
    protected override UnityAction OnConfirmed => () =>
    {
        if (NetworkManager.Instance.CurrentRoomState != GameState.Lobby)
        {
            return;
        }

        PhotonNetwork.AutomaticallySyncScene = false;
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
    };

    protected override void Init()
    {
        UIManager.Instance.RegisterUI(this);

        maxPlayerPlusButton.onClick.RemoveAllListeners();
        maxPlayerMinusButton.onClick.RemoveAllListeners();
        maxPlayerInput.onEndEdit.RemoveAllListeners();
        //timeLimitInput.onValueChanged.RemoveAllListeners();
        //timeLimitInput.onEndEdit.RemoveAllListeners();

        privateCheckToggle.onValueChanged.RemoveAllListeners();
        passwordInput.onEndEdit.RemoveAllListeners();

        readyToggle.onValueChanged.RemoveAllListeners();
    }

    public override async void Show()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        await Setup();
        base.Show();
    }

    private void Update()
    {
        if (!PhotonNetwork.InRoom)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.F5) && !UIManager.Instance.GetUI<TutorialUI>().IsOpenned())
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
        UpdateDifficultyInput();
        UpdatePrivateRoomToggle();
        UpdatePasswordInput();
    }

    private async UniTask Setup()
    {
        foreach (PlayerInfoObj player in playerInfoObjects)
        {
            player.InfoToggle.isOn = false;
        }

        maxPlayerInput.interactable = false;
        //timeLimitInput.interactable = false;
        readyToggle.interactable = false;
        gameStartButton.interactable = false;

        readyToggle.gameObject.SetActive(false);
        gameStartButton.gameObject.SetActive(false);

        await UniTask.WaitUntil(() => PhotonNetwork.InRoom, cancellationToken: this.GetCancellationTokenOnDestroy());

        if (PhotonNetwork.IsMasterClient)
        {
            gameStartButton.gameObject.SetActive(true);

            maxPlayerInput.interactable = true;
            //timeLimitInput.interactable = true;

            maxPlayerPlusButton.onClick.AddListener(OnClickMaxPlayerPlusButton);
            maxPlayerMinusButton.onClick.AddListener(OnClickMaxPlayerMinusButton);
            maxPlayerInput.onEndEdit.AddListener(OnEndEditMaxPlayerInput);

            //timeLimitInput.onValidateInput += ValidateTimeLimitChar;
            //timeLimitInput.onValueChanged.AddListener(OnValueChangedTimeLimitInput);
            //timeLimitInput.onEndEdit.AddListener(OnEndEditTimeLimitInput);

            privateCheckToggle.interactable = true;
            privateCheckToggle.onValueChanged.AddListener(OnValueChangedPrivateCheckToggle);

            passwordInput.interactable = isPrivate;
            passwordInput.onValidateInput += CharUtils.ValidatePasswordChar;
            passwordInput.onEndEdit.AddListener(OnEndEditPassword);
        }
        else
        {
            privateCheckToggle.interactable = false;

            readyToggle.gameObject.SetActive(true);

            readyToggle.isOn = false;
            readyToggle.interactable = true;

            readyToggle.onValueChanged.AddListener(OnValueChangedReadyToggle);
        }

        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        curMaxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
        maxPlayerInput.text = curMaxPlayers.ToString();

        curDifficulty = (GameDifficulty)(int)PhotonNetwork.CurrentRoom.CustomProperties.Get(GameConstants.Network.GAME_DIFFICULTY_KEY, -1);
        difficultyInput.text = curDifficulty.ToString();

        isPrivate = (bool)PhotonNetwork.CurrentRoom.CustomProperties.Get(GameConstants.Network.ROOM_PRIVATE_KEY, false);
        password = (string)PhotonNetwork.CurrentRoom.CustomProperties.Get(GameConstants.Network.ROOM_PASSWORD_KEY, null);

        privateCheckToggle.isOn = isPrivate;
        passwordInput.text = password ?? "";

        //totalTimeLimitSeconds = (int)PhotonNetwork.CurrentRoom.CustomProperties[GameConstants.Network.ROOM_TIME_LIMIT_KEY];
        //int min = totalTimeLimitSeconds / 60;
        //int sec = totalTimeLimitSeconds % 60;
        //timeLimitInput.text = $"{min:00} : {sec:00}";
    }

    private void OnClickGameStartButton()
    {
        //UIManager.Instance.HideRoomLobbyUI();

        // #Critical
        // Load the Room Level.
        PhotonNetwork.MasterClient.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
        {
            { GameConstants.Network.PLAYER_NUMBER_KEY, 0 }
        });

        NetworkManager.Instance.SetGameState(GameState.Boot);
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
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        if (curMaxPlayers >= GameConstants.Network.MAX_PLAYERS_PER_ROOM)
        {
            return;
        }

        curMaxPlayers++;
    }

    private void OnClickMaxPlayerMinusButton()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        if (curMaxPlayers <= GameConstants.Network.MIN_PLAYERS_PER_ROOM)
        {
            return;
        }

        curMaxPlayers--;
    }

    private void OnEndEditMaxPlayerInput(string value)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            maxPlayerInput.text = curMaxPlayers.ToString();
            return;
        }

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

    //private char ValidateTimeLimitChar(string text, int charIndex, char addedChar)
    //{
    //    if (char.IsDigit(addedChar))
    //    {
    //        return addedChar;
    //    }

    //    return '\0';
    //}

    //private void OnValueChangedTimeLimitInput(string value)
    //{
    //}

    //private void OnEndEditTimeLimitInput(string value)
    //{
    //    if (string.IsNullOrEmpty(value))
    //    {
    //        totalTimeLimitSeconds = 0;
    //        UpdateTimeLimit();
    //        return;
    //    }

    //    string digits = "";
    //    foreach (char c in value)
    //    {
    //        if (char.IsDigit(c))
    //        {
    //            digits += c;
    //        }
    //    }

    //    digits = digits.PadRight(4, '0')[..4];
    //    value = digits.Insert(2, ":");

    //    if (!value.TryParseTime(out totalTimeLimitSeconds))
    //    {
    //        totalTimeLimitSeconds = 0;
    //    }

    //    UpdateTimeLimit();
    //}

    //private void OnClickTimeLimitPlusButton()
    //{
    //    totalTimeLimitSeconds++;
    //    totalTimeLimitSeconds = Mathf.Clamp(totalTimeLimitSeconds, 0, 59 * 60 + 59);
    //    UpdateTimeLimit();
    //}

    //private void OnClickTimeLimitMinusButton()
    //{
    //    totalTimeLimitSeconds--;
    //    totalTimeLimitSeconds = Mathf.Clamp(totalTimeLimitSeconds, 0, 59 * 60 + 59);
    //    UpdateTimeLimit();
    //}

    //private void UpdateTimeLimit()
    //{
    //    if (!PhotonNetwork.IsMasterClient)
    //    {
    //        return;
    //    }

    //    PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable
    //    {
    //        { GameConstants.Network.ROOM_TIME_LIMIT_KEY, totalTimeLimitSeconds }
    //    });
    //}

    private void OnClickDifficultyPlusButton()
    {
        if ((int)curDifficulty == Enum.GetValues(typeof(GameDifficulty)).Length - 2)
        {
            return;
        }

        curDifficulty++;
        SetDifficulty();
    }

    private void OnClickDifficultyMinusButton()
    {
        if (curDifficulty == 0)
        {
            return;
        }

        curDifficulty--;
        SetDifficulty();
    }

    private void SetDifficulty()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable
        {
            { GameConstants.Network.GAME_DIFFICULTY_KEY, (int)curDifficulty }
        });
    }

    private void UpdateDifficultyInput()
    {
        difficultyInput.text = curDifficulty.ToString();
    }

    private void OnValueChangedPrivateCheckToggle(bool isOn)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        isPrivate = isOn;
        passwordInput.interactable = isPrivate;

        SetPrivateRoom();
    }

    private void SetPrivateRoom()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable
        {
            { GameConstants.Network.ROOM_PRIVATE_KEY, isPrivate }
        });
    }

    private void UpdatePrivateRoomToggle()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            return;
        }

        privateCheckToggle.isOn = isPrivate;
    }

    private void OnEndEditPassword(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            passwordInput.text = password;
            return;
        }

        password = value;
        SetPassword();
    }

    private void SetPassword()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable
        {
            { GameConstants.Network.ROOM_PASSWORD_KEY, password }
        });
    }

    private void UpdatePasswordInput()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            return;
        }

        passwordInput.text = password;
    }

    public void OnUpdatedCustomProperties(Hashtable propertiesThatChanged)
    {
        if (!IsOpenned())
        {
            return;
        }

        //if (propertiesThatChanged.TryGetValue(GameConstants.Network.ROOM_TIME_LIMIT_KEY, out object timeLimit))
        //{
        //    totalTimeLimitSeconds = (int)timeLimit;

        //    int min = totalTimeLimitSeconds / 60;
        //    int sec = totalTimeLimitSeconds % 60;
        //    timeLimitInput.text = $"{min:00} : {sec:00}";
        //}

        if (propertiesThatChanged.TryGetValue(GameConstants.Network.GAME_DIFFICULTY_KEY, out object difficulty))
        {
            curDifficulty = (GameDifficulty)(int)difficulty;
        }

        if (propertiesThatChanged.TryGetValue(GameConstants.Network.ROOM_PRIVATE_KEY, out object isPrivate))
        {
            this.isPrivate = (bool)isPrivate;
        }

        if (propertiesThatChanged.TryGetValue(GameConstants.Network.ROOM_PASSWORD_KEY, out object password))
        {
            this.password = (string)password;
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

            if (player.CustomProperties.ContainsKey(GameConstants.Network.PLAYER_STATUS_KEY))
            {
                playerStatusDict[player.UserId] = (PlayerInfoObj.Status)(int)player.CustomProperties[GameConstants.Network.PLAYER_STATUS_KEY];
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
            NetworkManager.Instance.SetLobbyPlayerStatus(PhotonNetwork.LocalPlayer, PlayerInfoObj.Status.Ready);
            //playerInfoObjects.Find(p => p.Info == PhotonNetwork.LocalPlayer).Init(PhotonNetwork.LocalPlayer);
            Setup().Forget();
        }
    }

    private void OnValueChangedReadyToggle(bool isOn)
    {
        NetworkManager.Instance.SetLobbyPlayerStatus(PhotonNetwork.LocalPlayer, isOn ? PlayerInfoObj.Status.Ready : PlayerInfoObj.Status.NotReady);
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
        NetworkManager.Instance.SetLobbyPlayerStatus(PhotonNetwork.LocalPlayer, PlayerInfoObj.Status.Busy);
        UIManager.Instance.ShowUI<TutorialUI>(hidePrev: false);
    }

    private async UniTaskVoid CalcBusyCool()
    {
        isBusyCool = true;
        await UniTask.Delay(GameConstants.Network.LOBBY_BUSY_COOL_TIME, cancellationToken: this.GetCancellationTokenOnDestroy());
        isBusyCool = false;
    }
}
