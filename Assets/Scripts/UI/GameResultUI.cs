using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace Shurub
{
    public class GameResultUI : UIBase<GameResultUI>
    {
        [SerializeField] private Button retryButton;
        [SerializeField] private Button goToLobbyButton;
        [SerializeField] private Button leaveButton;

        private void OnClickRetryButton()
        {
            NetworkManager.Instance.SetGameState(GameState.Retry);
        }

        private void OnClickGoToLobbyButton()
        {
            GameManager.Instance.GoToRoomLobby();
        }

        private void OnClickLeaveButton()
        {
            GameManager.Instance.GoToMain();
        }
    }
}
