using Cysharp.Threading.Tasks;
using Shurub;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinRoomUI : UIBase
{
    [SerializeField] private Button searchRoomButton;
    [SerializeField] private TMP_InputField searchRoomInput;

    [SerializeField] private ToggleGroup roomListToggleGroup;
    [SerializeField] private RoomInfoObj roomInfoPrefab;

    private string curSearchRoomStr = "";

    private List<RoomInfo> cachedAvailableRooms;
    private List<RoomInfoObj> roomInfoObjects = new List<RoomInfoObj>();

    protected override void Init()
    {
        UIManager.Instance().RegisterUI(this);

        searchRoomInput.onValueChanged.RemoveAllListeners();
        searchRoomInput.onValueChanged.AddListener(OnValueChangedSearchRoomInput);
    }

    public override void Show()
    {
        foreach (RoomInfoObj room in roomInfoObjects)
        {
            room.InfoToggle.isOn = false;
        }

        base.Show();
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
            if (room.Name.Contains(curSearchRoomStr))
            {
                RoomInfoObj roomObj = roomInfoObjects[i];
                roomObj.gameObject.SetActive(true);
                roomObj.Init(room);
            }
        }
    }
}
