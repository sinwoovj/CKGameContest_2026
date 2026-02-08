using Cysharp.Threading.Tasks;
using Shurub;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class PlayUI : UIBase
{
    [SerializeField] private Button singleButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button tutorialButton;

    protected override void Init()
    {
        UIManager.Instance.RegisterUI(this);
    }

    public override void Show()
    {
        ProcessShow().Forget();
    }

    private async UniTaskVoid ProcessShow()
    {
        base.Show();

        hostButton.interactable = false;
        joinButton.interactable = false;

        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }

        await UniTask.WaitUntil(() => PhotonNetwork.InLobby, cancellationToken: this.GetCancellationTokenOnDestroy());

        hostButton.interactable = true;
        joinButton.interactable = true;
    }

    private void OnClickSingleButton()
    {

    }

    private void OnClickHostButton()
    {
        UIManager.Instance.ShowUI<HostRoomUI>();
    }

    private void OnClickJoinButton()
    {
        UIManager.Instance.ShowUI<JoinRoomUI>();
    }

    private void OnClickTutorialButton()
    {
        UIManager.Instance.ShowUI<TutorialUI>();
    }
}
