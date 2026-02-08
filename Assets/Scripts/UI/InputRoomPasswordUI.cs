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

            passwordInput.onValidateInput += ValidatePasswordChar;
        }

        public void Setup(RoomInfo room)
        {
            targetRoom = room;
            roomNameText.text = targetRoom.Name;
        }

        private char ValidatePasswordChar(string text, int charIndex, char addedChar)
        {
            if (text.Length >= passwordInput.characterLimit) return '\0';                           // 길이 제한
            if (char.IsWhiteSpace(addedChar)) return '\0';                                          // 공백
            if (addedChar >= 0xAC00 && addedChar <= 0xD7A3) return '\0';                            // 한글
            if (char.IsSurrogate(addedChar)) return '\0';                                           // 이모지
            if (!(char.IsLetterOrDigit(addedChar) || "!@#$%^&*".Contains(addedChar))) return '\0';  // 일부 특수문자

            return addedChar;
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
