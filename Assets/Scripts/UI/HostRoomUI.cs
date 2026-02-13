using Cysharp.Threading.Tasks;
using DG.Tweening;
using Shurub;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using System.Linq;

public class HostRoomUI : UIBase<HostRoomUI>
{
    [SerializeField] private Button hostRoomButton;

    [SerializeField] private Button maxPlayerPlusButton;
    [SerializeField] private Button maxPlayerMinusButton;
    [SerializeField] private TMP_InputField maxPlayerInput;

    [SerializeField] private TMP_InputField roomNameInput;

    [SerializeField] private Toggle privateCheckToggle;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Toggle showPasswordToggle;

    //[SerializeField] private Button timeLimitPlusButton;
    //[SerializeField] private Button timeLimitMinusButton;
    //[SerializeField] private TMP_InputField timeLimitInput;

    [SerializeField] private Button difficultyPlusButton;
    [SerializeField] private Button difficultyMinusButton;
    [SerializeField] private TMP_InputField difficultyInput;

    [SerializeField] private CanvasGroup hostingPanel;

    private bool isHosting = false;
    private int curMaxPlayers = GameConstants.Network.MIN_PLAYERS_PER_ROOM;

    private string password = "";

    private GameDifficulty curDifficulty = GameDifficulty.Easy;
    //private int totalTimeLimitSeconds = 0;

    private const float HOSTING_PANEL_FADE_DURATION = 0.1f;

    protected override void OnAwake()
    {
        maxPlayerInput.onEndEdit.RemoveAllListeners();
        maxPlayerInput.onEndEdit.AddListener(OnEndEditMaxPlayerInput);

        privateCheckToggle.onValueChanged.RemoveAllListeners();
        privateCheckToggle.onValueChanged.AddListener(OnValueChangedPrivateCheckToggle);

        passwordInput.onValidateInput += CharUtils.ValidatePasswordChar;
        passwordInput.onEndEdit.RemoveAllListeners();
        passwordInput.onEndEdit.AddListener(OnEndEditPassword);

        showPasswordToggle.onValueChanged.RemoveAllListeners();
        showPasswordToggle.onValueChanged.AddListener(OnValueChangedShowPasswordToggle);

        //timeLimitInput.onValidateInput += ValidateTimeLimitChar;
        //timeLimitInput.onValueChanged.RemoveAllListeners();
        //timeLimitInput.onValueChanged.AddListener(OnValueChangedTimeLimitInput);
        //timeLimitInput.onEndEdit.RemoveAllListeners();
        //timeLimitInput.onEndEdit.AddListener(OnEndEditTimeLimitInput);
    }

    public override void Show()
    {
        ProcessShow().Forget();
    }

    private async UniTaskVoid ProcessShow()
    {
        base.Show();

        isHosting = false;
        curMaxPlayers = GameConstants.Network.MIN_PLAYERS_PER_ROOM;
        curDifficulty = GameDifficulty.Easy;
        password = "";
        //totalTimeLimitSeconds = 0;

        roomNameInput.text = "";
        privateCheckToggle.isOn = false;
        passwordInput.interactable = false;
        passwordInput.text = "";
        showPasswordToggle.isOn = false;

        difficultyInput.interactable = false;
        difficultyInput.text = "";

        hostingPanel.alpha = 0f;
        hostingPanel.interactable = false;
        hostingPanel.blocksRaycasts = false;
        hostRoomButton.interactable = false;

        await UniTask.WaitUntil(() => PhotonNetwork.InLobby, cancellationToken: this.GetCancellationTokenOnDestroy());

        hostRoomButton.interactable = true;
    }

    private void OnClickHostRoomButton()
    {
        if (isHosting || curMaxPlayers < GameConstants.Network.MIN_PLAYERS_PER_ROOM)
        {
            return;
        }

        hostingPanel.DOFade(1f, HOSTING_PANEL_FADE_DURATION).OnComplete(() =>
        {
            hostingPanel.interactable = true;
            hostingPanel.blocksRaycasts = true;
        });

        hostRoomButton.interactable = false;
        isHosting = true;

        if (privateCheckToggle.isOn && string.IsNullOrEmpty(password))
        {
            password = Guid.NewGuid().ToString("N");
        }

        string name = string.IsNullOrEmpty(roomNameInput.text) ? Guid.NewGuid().ToString()[..16] : roomNameInput.text;
        RoomOptions options = new RoomOptions
        {
            IsOpen = true,
            IsVisible = true,
            PublishUserId = true,
            MaxPlayers = curMaxPlayers,
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
            {
                { GameConstants.Network.ROOM_PRIVATE_KEY, privateCheckToggle.isOn },
                { GameConstants.Network.ROOM_PASSWORD_KEY, password },
                { GameConstants.Network.GAME_STATE_KEY, GameState.Lobby },
                { GameConstants.Network.GAME_DIFFICULTY_KEY, (int)curDifficulty }
                //{ GameConstants.Network.ROOM_TIME_LIMIT_KEY, totalTimeLimitSeconds }
            },
            CustomRoomPropertiesForLobby = new string[]
            {
                GameConstants.Network.ROOM_PRIVATE_KEY,
                GameConstants.Network.ROOM_PASSWORD_KEY,
                GameConstants.Network.GAME_DIFFICULTY_KEY
            }
        };
        PhotonNetwork.CreateRoom(name, options);
    }

    private void OnClickMaxPlayerPlusButton()
    {
        if (curMaxPlayers >= GameConstants.Network.MAX_PLAYERS_PER_ROOM)
        {
            return;
        }

        curMaxPlayers++;
        UpdateMaxPlayerInput();
    }

    private void OnClickMaxPlayerMinusButton()
    {
        if (curMaxPlayers <= GameConstants.Network.MIN_PLAYERS_PER_ROOM)
        {
            return;
        }

        curMaxPlayers--;
        UpdateMaxPlayerInput();
    }

    private void OnEndEditMaxPlayerInput(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            UpdateMaxPlayerInput();
            return;
        }

        if (!int.TryParse(value, out int number))
        {
            UpdateMaxPlayerInput();
            return;
        }

        curMaxPlayers = Mathf.Clamp(number, GameConstants.Network.MIN_PLAYERS_PER_ROOM, GameConstants.Network.MAX_PLAYERS_PER_ROOM);
        UpdateMaxPlayerInput();
    }

    private void UpdateMaxPlayerInput()
    {
        maxPlayerInput.text = curMaxPlayers.ToString();
    }

    private void OnClickDifficultyPlusButton()
    {
        if ((int)curDifficulty == Enum.GetValues(typeof(GameDifficulty)).Length - 2)
        {
            return;
        }

        curDifficulty++;
        UpdateDifficultyInput();
    }

    private void OnClickDifficultyMinusButton()
    {
        if (curDifficulty == 0)
        {
            return;
        }

        curDifficulty--;
        UpdateDifficultyInput();
    }

    private void UpdateDifficultyInput()
    {
        difficultyInput.text = curDifficulty.ToString();
    }

    private void OnValueChangedPrivateCheckToggle(bool isOn)
    {
        passwordInput.interactable = isOn;
        //if (!isOn)
        //{
        //    password = "";
        //    passwordInput.text = "";
        //}
    }

    private void OnEndEditPassword(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        password = value;
    }

    private void OnValueChangedShowPasswordToggle(bool isOn)
    {
        passwordInput.contentType = isOn ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
        passwordInput.inputType = isOn ? TMP_InputField.InputType.Standard : TMP_InputField.InputType.Password;
        passwordInput.ForceLabelUpdate();
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
    //        UpdateTimeLimitInput();
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

    //    UpdateTimeLimitInput();
    //}

    //private void OnClickTimeLimitPlusButton()
    //{
    //    totalTimeLimitSeconds++;
    //    totalTimeLimitSeconds = Mathf.Clamp(totalTimeLimitSeconds, 0, 59 * 60 + 59);
    //    UpdateTimeLimitInput();
    //}

    //private void OnClickTimeLimitMinusButton()
    //{
    //    totalTimeLimitSeconds--;
    //    totalTimeLimitSeconds = Mathf.Clamp(totalTimeLimitSeconds, 0, 59 * 60 + 59);
    //    UpdateTimeLimitInput();
    //}

    //private void UpdateTimeLimitInput()
    //{
    //    int min = totalTimeLimitSeconds / 60;
    //    int sec = totalTimeLimitSeconds % 60;
    //    timeLimitInput.text = $"{min:00} : {sec:00}";
    //}
}
