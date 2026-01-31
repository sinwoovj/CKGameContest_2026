using JJM;
using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : UIBase
{
    [SerializeField] private Button gameRuleOpenButton;
    [SerializeField] private Button controlOpenButton;

    [SerializeField] private Button gameRuleCloseButton;
    [SerializeField] private Button controlCloseButton;

    protected override void Init()
    {
        UIManager.Instance().RegisterUI(this);
    }

    public override void Show()
    {
        gameRuleOpenButton.gameObject.SetActive(true);
        controlOpenButton.gameObject.SetActive(true);

        gameRuleCloseButton.gameObject.SetActive(false);
        controlCloseButton.gameObject.SetActive(false);

        base.Show();
    }

    public override void Hide(Action onConfirmed, bool force)
    {
        if (PhotonNetwork.InRoom)
        {
            NetworkManager.Instance().SetLobbyPlayerStatus(PhotonNetwork.LocalPlayer, PlayerInfoObj.Status.NotReady);
        }

        base.Hide(onConfirmed, force);
    }

    private void OnClickGameRuleOpenButton()
    {
        gameRuleOpenButton.gameObject.SetActive(false);
        gameRuleCloseButton.gameObject.SetActive(true);
    }

    private void OnClickGameRuleCloseButton()
    {
        gameRuleCloseButton.gameObject.SetActive(false);
        gameRuleOpenButton.gameObject.SetActive(true);
    }

    private void OnClickControlOpenButton()
    {
        controlOpenButton.gameObject.SetActive(false);
        controlCloseButton.gameObject.SetActive(true);
    }

    private void OnClickControlCloseButton()
    {
        controlCloseButton.gameObject.SetActive(false);
        controlOpenButton.gameObject.SetActive(true);
    }
}
