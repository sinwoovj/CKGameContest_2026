using Cysharp.Threading.Tasks;
using Shurub;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class TitleUI : UIBase
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button exitButton;

    protected override void Init()
    {
        UIManager.Instance().RegisterUI(this);
    }

    public override void Show()
    {
        ProcessShow().Forget();
    }

    private async UniTaskVoid ProcessShow()
    {
        base.Show();

        playButton.interactable = false;
        await UniTask.WaitUntil(() => PhotonNetwork.IsConnectedAndReady, cancellationToken: this.GetCancellationTokenOnDestroy());
        playButton.interactable = true;
    }

    private void OnClickPlayButton()
    {
        UIManager.Instance().ShowUI<PlayUI>();
    }

    private void OnClickSettingButton()
    {

    }

    private void OnClickExitButton()
    {
        ModalManager.Instance().OpenNewModal("확인", "게임을 종료 하시겠습니까?", yesAction: Application.Quit);
    }
}
