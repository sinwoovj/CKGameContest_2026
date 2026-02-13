using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace Shurub
{
    public class GamePauseUI : UIBase<GamePauseUI>
    {
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button playerManagementButton;
        [SerializeField] private Button settingButton;
        [SerializeField] private Button leaveButton;

        public override void Show()
        {
            InGameManager.Instance.LocalPlayer.GetComponent<PlayerController>().playerInput.enabled = false;
            if (PhotonNetwork.IsMasterClient)
            {
                playerManagementButton.interactable = true;
            }
            else
            {
                playerManagementButton.interactable = false;
            }

            base.Show();
        }

        public override void Hide()
        {
            InGameManager.Instance.LocalPlayer.GetComponent<PlayerController>().playerInput.enabled = true;
            base.Hide();
        }

        private void OnClickResumeButton()
        {
            UIManager.Instance.HideUI<GamePauseUI>();
        }

        private void OnClickPlayerManagementButton()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            UIManager.Instance.ShowUI<GamePlayerManagementUI>();
        }

        private async void OnClickSettingButton()
        {
            await UIManager.Instance.CheckAndMakeUI<SettingUI>(GameConstants.UI.SETTING_UI_PATH);
            UIManager.Instance.ShowUI<SettingUI>(hidePrev: false);
        }

        private void OnClickLeaveButton()
        {
            GameManager.Instance.GoToMain();
        }
    }
}
