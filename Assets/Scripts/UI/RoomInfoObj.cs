using Photon.Pun;
using Photon.Realtime;
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

        private bool isJoining = false;

        public Toggle InfoToggle => infoToggle;

        public void Init(RoomInfo info)
        {
            isJoining = false;
            infoToggle.onValueChanged.RemoveAllListeners();
            infoToggle.onValueChanged.AddListener(OnValueChangedInfoToggle);

            int total = (int)info.CustomProperties.Get(GameConstants.Network.ROOM_TIME_LIMIT_HASH_PROP, 0);
            int min = total / 60;
            int sec = total % 60;
            string time = $"{min:00} : {sec:00}";

            idText.text = info.Name;
            settingText.text = $"Max Players: {info.MaxPlayers}, Time Limit: {time}";

            joinButton.onClick.RemoveAllListeners();
            joinButton.onClick.AddListener(() =>
            {
                if (isJoining)
                {
                    return;
                }

                isJoining = true;
                PhotonNetwork.JoinRoom(info.Name);
            });
        }

        private void OnValueChangedInfoToggle(bool isOn)
        {
            infoToggle.targetGraphic.color = isOn ? infoToggle.colors.selectedColor : infoToggle.colors.normalColor;
            // Todo: 방 정보 보여주기
        }
    }
}
