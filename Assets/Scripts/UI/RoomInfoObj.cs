using Photon.Pun;
using Photon.Realtime;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shurub
{
    public class RoomInfoObj : MonoBehaviour
    {
        [SerializeField] private Toggle infoToggle;
        [SerializeField] private Button joinButton;
        [SerializeField] private TMP_Text idText;
        [SerializeField] private TMP_Text settingText;
        [SerializeField] private Image privateImage;

        private bool isJoining = false;

        public Toggle InfoToggle => infoToggle;

        public void Init(RoomInfo info)
        {
            isJoining = false;
            infoToggle.onValueChanged.RemoveAllListeners();
            infoToggle.onValueChanged.AddListener(OnValueChangedInfoToggle);

            //int total = (int)info.CustomProperties.Get(GameConstants.Network.ROOM_TIME_LIMIT_KEY, 0);
            //int min = total / 60;
            //int sec = total % 60;
            //string time = $"{min:00} : {sec:00}";

            GameDifficulty difficulty = (GameDifficulty)(int)info.CustomProperties.Get(GameConstants.Network.GAME_DIFFICULTY_KEY, -1);

            idText.text = info.Name;
            //settingText.text = $"Max Players: {info.MaxPlayers}, Time Limit: {time}";
            settingText.text = $"Max Players: {info.MaxPlayers}, Difficulty: {difficulty}";

            bool isPrivate = (bool)info.CustomProperties.Get(GameConstants.Network.ROOM_PRIVATE_KEY, false);
            privateImage.gameObject.SetActive(isPrivate);

            joinButton.onClick.RemoveAllListeners();
            joinButton.onClick.AddListener(() =>
            {
                if (isJoining)
                {
                    return;
                }

                if (isPrivate)
                {
                    UIManager.Instance.GetUI<InputRoomPasswordUI>().Setup(info);
                    UIManager.Instance.ShowUI<InputRoomPasswordUI>(hidePrev: false);
                }
                else
                {
                    isJoining = true;
                    PhotonNetwork.JoinRoom(info.Name, null);
                }
            });
        }

        private void OnValueChangedInfoToggle(bool isOn)
        {
            infoToggle.targetGraphic.color = isOn ? infoToggle.colors.selectedColor : infoToggle.colors.normalColor;
            // Todo: 방 정보 보여주기
        }
    }
}
