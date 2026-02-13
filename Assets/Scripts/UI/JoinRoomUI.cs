using Cysharp.Threading.Tasks;
using Shurub;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinRoomUI : UIBase<JoinRoomUI>
{
    [SerializeField] private Button searchRoomButton;
    [SerializeField] private TMP_InputField searchRoomInput;

    [SerializeField] private ToggleGroup roomListToggleGroup;
    [SerializeField] private RoomInfoObj roomInfoPrefab;

    [SerializeField] private Toggle privateCheckToggle;

    private string curSearchRoomStr = "";

    private List<RoomInfo> cachedAvailableRooms = new List<RoomInfo>();
    private List<RoomInfoObj> roomInfoObjects = new List<RoomInfoObj>();

    protected override void OnAwake()
    {
        searchRoomInput.onValueChanged.RemoveAllListeners();
        searchRoomInput.onValueChanged.AddListener(OnValueChangedSearchRoomInput);
        privateCheckToggle.onValueChanged.RemoveAllListeners();
        privateCheckToggle.onValueChanged.AddListener(OnValueChangedPrivateCheckToggle);
    }

    public override void Show()
    {
        base.Show();

        OnUpdatedRoomList(cachedAvailableRooms);
        foreach (RoomInfoObj room in roomInfoObjects)
        {
            room.InfoToggle.isOn = false;
        }
    }

    private void OnClickSearchRoomButton()
    {
        searchRoomInput.ActivateInputField();
    }

    private void OnValueChangedSearchRoomInput(string value)
    {
        curSearchRoomStr = value;
        OnUpdatedRoomList(cachedAvailableRooms);
    }

    public void OnUpdatedRoomList(List<RoomInfo> roomList)
    {
        cachedAvailableRooms = roomList;

        if (!UIManager.Instance.IsOpenned(this))
        {
            return;
        }

        for (int i = 0; i < roomInfoObjects.Count; i++)
        {
            roomInfoObjects[i].gameObject.SetActive(false);
        }

        for (int i = roomInfoObjects.Count; i < roomList.Count; i++)
        {
            RoomInfoObj roomObj = Instantiate(roomInfoPrefab, roomListToggleGroup.transform);
            roomObj.InfoToggle.group = roomListToggleGroup;
            roomObj.gameObject.SetActive(false);
            roomInfoObjects.Add(roomObj);
        }

        for (int i = 0; i < roomList.Count; i++)
        {
            RoomInfo room = roomList[i];
            if (!privateCheckToggle.isOn && (bool)room.CustomProperties.Get(GameConstants.Network.ROOM_PRIVATE_KEY, false))
            {
                continue;
            }

            if (room.Name.Contains(curSearchRoomStr))
            {
                RoomInfoObj roomObj = roomInfoObjects[i];
                roomObj.gameObject.SetActive(true);
                roomObj.Init(room);
            }
        }
    }

    private void OnValueChangedPrivateCheckToggle(bool isOn)
    {
        OnUpdatedRoomList(cachedAvailableRooms);
    }
}
