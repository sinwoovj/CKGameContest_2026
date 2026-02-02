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

public class HostRoomUI : UIBase
{
    [SerializeField] private Button hostRoomButton;

    [SerializeField] private Button maxPlayerPlusButton;
    [SerializeField] private Button maxPlayerMinusButton;
    [SerializeField] private TMP_InputField maxPlayerInput;

    [SerializeField] private TMP_InputField roomIdInput;

    [SerializeField] private Button timeLimitPlusButton;
    [SerializeField] private Button timeLimitMinusButton;
    [SerializeField] private TMP_InputField timeLimitInput;

    [SerializeField] private CanvasGroup hostingPanel;

    private bool isHosting = false;
    private int curMaxPlayers = GameConstants.Network.MIN_PLAYERS_PER_ROOM;
    private int totalTimeLimitSeconds = 0;

    private const float HOSTING_PANEL_FADE_DURATION = 0.1f;

    protected override void Init()
    {
        UIManager.Instance().RegisterUI(this);

        maxPlayerInput.onEndEdit.RemoveAllListeners();
        maxPlayerInput.onEndEdit.AddListener(OnEndEditMaxPlayerInput);

        timeLimitInput.onValidateInput += ValidateTimeLimitChar;
        timeLimitInput.onValueChanged.RemoveAllListeners();
        timeLimitInput.onValueChanged.AddListener(OnValueChangedTimeLimitInput);
        timeLimitInput.onEndEdit.RemoveAllListeners();
        timeLimitInput.onEndEdit.AddListener(OnEndEditTimeLimitInput);
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
        totalTimeLimitSeconds = 0;

        roomIdInput.text = "";
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

        string name = string.IsNullOrEmpty(roomIdInput.text) ? Guid.NewGuid().ToString()[..16] : roomIdInput.text;
        RoomOptions options = new RoomOptions
        {
            IsOpen = true,
            IsVisible = true,
            PublishUserId = true,
            MaxPlayers = curMaxPlayers,
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
            {
                { GameConstants.Network.ROOM_TIME_LIMIT_HASH_PROP, totalTimeLimitSeconds }
            },
            CustomRoomPropertiesForLobby = new string[] { GameConstants.Network.ROOM_TIME_LIMIT_HASH_PROP }
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
            UpdateTimeLimitInput();
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

        UpdateTimeLimitInput();
    }

    private void OnClickTimeLimitPlusButton()
    {
        totalTimeLimitSeconds++;
        totalTimeLimitSeconds = Mathf.Clamp(totalTimeLimitSeconds, 0, 59 * 60 + 59);
        UpdateTimeLimitInput();
    }

    private void OnClickTimeLimitMinusButton()
    {
        totalTimeLimitSeconds--;
        totalTimeLimitSeconds = Mathf.Clamp(totalTimeLimitSeconds, 0, 59 * 60 + 59);
        UpdateTimeLimitInput();
    }

    private void UpdateTimeLimitInput()
    {
        int min = totalTimeLimitSeconds / 60;
        int sec = totalTimeLimitSeconds % 60;
        timeLimitInput.text = $"{min:00} : {sec:00}";
    }
}
