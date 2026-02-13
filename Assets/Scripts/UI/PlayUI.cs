using Cysharp.Threading.Tasks;
using Shurub;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class PlayUI : UIBase<PlayUI>
{
    [SerializeField] private Button singleButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button tutorialButton;

    public override void Show()
    {
        base.Show();

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            return;
        }

        if (!PhotonNetwork.InLobby && PhotonNetwork.NetworkClientState != Photon.Realtime.ClientState.JoiningLobby)
        {
            PhotonNetwork.JoinLobby();
        }
    }

    private void OnClickSingleButton()
    {

    }

    private void OnClickHostButton()
    {
        if (!PhotonNetwork.InLobby)
        {
            return;
        }

        UIManager.Instance.ShowUI<HostRoomUI>();
    }

    private void OnClickJoinButton()
    {
        if (!PhotonNetwork.InLobby)
        {
            return;
        }

        UIManager.Instance.ShowUI<JoinRoomUI>();
    }

    private void OnClickTutorialButton()
    {
        UIManager.Instance.ShowUI<TutorialUI>();
    }
}
