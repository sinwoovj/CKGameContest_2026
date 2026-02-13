using Cysharp.Threading.Tasks;
using Shurub;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class TitleUI : UIBase<TitleUI>
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button exitButton;

    public override void Show()
    {
        SoundManager.Instance.Play("MainBGM");
        base.Show();
    }

    public override void Hide()
    {
        base.Hide();
    }

    private void OnClickPlayButton()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            return;
        }

        UIManager.Instance.ShowUI<PlayUI>();
    }

    private async void OnClickSettingButton()
    {
        await UIManager.Instance.CheckAndMakeUI<SettingUI>(GameConstants.UI.SETTING_UI_PATH);
        UIManager.Instance.ShowUI<SettingUI>(hidePrev: false);
    }

    private void OnClickExitButton()
    {
        ModalManager.Instance.OpenNewModal("확인", "게임을 종료 하시겠습니까?", yesAction: Application.Quit);
    }
}
