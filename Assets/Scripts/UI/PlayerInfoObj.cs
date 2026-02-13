using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shurub
{
    public class PlayerInfoObj : MonoBehaviour
    {
        [SerializeField] private Toggle infoToggle;
        [SerializeField] private Button kickButton;
        [SerializeField] private Image statusImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Image masterImage;

        public Photon.Realtime.Player Info { get; private set; }
        public Toggle InfoToggle => infoToggle;

        public void Init(Photon.Realtime.Player player)
        {
            Info = player;

            infoToggle.onValueChanged.RemoveAllListeners();
            infoToggle.onValueChanged.AddListener(OnValueChangedInfoToggle);

            nameText.text = player.NickName + (player == PhotonNetwork.LocalPlayer ? " (Me)" : "");
            masterImage.gameObject.SetActive(player.IsMasterClient);

            if (!PhotonNetwork.IsMasterClient || player == PhotonNetwork.LocalPlayer)
            {
                kickButton.gameObject.SetActive(false);
            }
            else
            {
                kickButton.gameObject.SetActive(true);
                kickButton.onClick.RemoveAllListeners();
                kickButton.onClick.AddListener(() =>
                {
                    NetworkManager.Instance.KickPlayer(player);
                });
            }

            statusImage.color = (PlayerStatus)(int)player.CustomProperties.Get(GameConstants.Network.PLAYER_STATUS_KEY, -1) switch
            {
                PlayerStatus.NotReady => (Color)new Color32(117, 117, 117, 255),
                PlayerStatus.Ready => (Color)new Color32(126, 217, 87, 255),
                PlayerStatus.Busy => (Color)new Color32(255, 45, 45, 255),
                _ => (Color)new Color32(255, 255, 255, 255),
            };
        }

        private void OnValueChangedInfoToggle(bool isOn)
        {
            infoToggle.targetGraphic.color = isOn ? infoToggle.colors.selectedColor : infoToggle.colors.normalColor;
            // Todo: 플레이어 정보 보여주기
        }
    }
}
