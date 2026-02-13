using Shurub;
using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : UIBase<TutorialUI>
{
    [SerializeField] private Button gameRuleOpenButton;
    [SerializeField] private Button controlOpenButton;

    [SerializeField] private Button gameRuleCloseButton;
    [SerializeField] private Button controlCloseButton;

    public override void Show()
    {
        gameRuleOpenButton.gameObject.SetActive(true);
        controlOpenButton.gameObject.SetActive(true);

        gameRuleCloseButton.gameObject.SetActive(false);
        controlCloseButton.gameObject.SetActive(false);

        base.Show();
    }

    public override void Hide()
    {
        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                NetworkManager.Instance.SetPlayerStatus(PhotonNetwork.LocalPlayer, PlayerStatus.Ready);
            }
            else
            {
                NetworkManager.Instance.SetPlayerStatus(PhotonNetwork.LocalPlayer, PlayerStatus.NotReady);
            }
        }

        base.Hide();
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
