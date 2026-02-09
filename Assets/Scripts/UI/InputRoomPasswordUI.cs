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
        [SerializeField] private Toggle showPasswordToggle;

        private bool isJoining = false;

        protected override void Init()
        {
            UIManager.Instance.RegisterUI(this);

            passwordInput.onValidateInput += CharUtils.ValidatePasswordChar;
            showPasswordToggle.onValueChanged.RemoveAllListeners();
            showPasswordToggle.onValueChanged.AddListener(OnValueChangedShowPasswordToggle);
        }

        public override void Show()
        {
            base.Show();

            showPasswordToggle.isOn = false;
            passwordInput.ActivateInputField();
        }

        public void Setup(RoomInfo room)
        {
            targetRoom = room;
            isJoining = false;
            roomNameText.text = targetRoom.Name;
        }

        private void OnValueChangedShowPasswordToggle(bool isOn)
        {
            passwordInput.contentType = isOn ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
            passwordInput.inputType = isOn ? TMP_InputField.InputType.Standard : TMP_InputField.InputType.Password;
            passwordInput.ForceLabelUpdate();
        }

        private void OnClickJoinButton()
        {
            if (isJoining || targetRoom == null || string.IsNullOrEmpty(passwordInput.text))
            {
                return;
            }

            if (targetRoom.CustomProperties.TryGetValue(GameConstants.Network.ROOM_PASSWORD_KEY, out object pw))
            {
                if (passwordInput.text == (string)pw)
                {
                    isJoining = true;
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
