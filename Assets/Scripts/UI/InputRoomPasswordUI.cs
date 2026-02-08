using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Shurub
{
    public class InputRoomPasswordUI : UIBase
    {
        private RoomInfo targetRoom;

        [SerializeField] private TMP_Text roomNameText;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button cancelButton;

        protected override void Init()
        {
            UIManager.Instance.RegisterUI(this);

            passwordInput.onValidateInput += CharUtils.ValidatePasswordChar;
        }

        public void Setup(RoomInfo room)
        {
            targetRoom = room;
            roomNameText.text = targetRoom.Name;
        }

        private void OnClickJoinButton()
        {
            if (targetRoom == null || string.IsNullOrEmpty(passwordInput.text))
            {
                return;
            }

            if (targetRoom.CustomProperties.TryGetValue(GameConstants.Network.ROOM_PASSWORD_KEY, out object pw))
            {
                if (passwordInput.text == (string)pw)
                {
                    PhotonNetwork.JoinRoom(targetRoom.Name, null);
                }
                else
                {
                    ModalManager.Instance.OpenNewModal("경고", "비밀번호가 틀립니다.", disableNo: true);
                }
            }
        }

        private void OnClickCancelButton()
        {
            UIManager.Instance.HideUI<InputRoomPasswordUI>();
        }
    }
}
