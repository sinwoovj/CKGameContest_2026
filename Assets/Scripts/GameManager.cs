using Photon.Pun;
using UnityEngine;

namespace Shurub
{
    public class GameManager : Singleton<GameManager>
    {
        public bool IsInitialized { get; private set; }

        public bool IsDevelopingVersion => Application.isEditor || Debug.isDebugBuild;

        [RuntimeInitializeOnLoadMethod]
        static void InitilizedOnLoaded()
        {
            NetworkManager.Instance.Init();

            UIManager.Instance.ClearAllUIs();
            if (Instance.IsDevelopingVersion)
            {
                UIManager.Instance.ShowUI<TitleUI>();
            }
            else
            {
                UIManager.Instance.ShowUI<IntroUI>();
            }
            
            Instance.IsInitialized = true;
        }

        public void GoToMain()
        {
            //NetworkManager.Instance.SetGameState(GameState.Lobby);
            PhotonNetwork.LeaveRoom();
            UIManager.Instance.ClearAllUIs();

            SceneManager.Instance.LoadScene(GameConstants.Scene.MAIN_SCENE_NAME, (scene, mode) =>
            {
                NetworkManager.Instance.CurrentRoomState = GameState.None;
                UIManager.Instance.ClearAllUIs();
                UIManager.Instance.ShowUI<TitleUI>();
            });
        }

        public void GoToRoomLobby()
        {
            NetworkManager.Instance.SetGameState(GameState.Lobby);
            UIManager.Instance.ClearAllUIs();

            SceneManager.Instance.LoadScene(GameConstants.Scene.MAIN_SCENE_NAME, (scene, mode) =>
            {
                UIManager.Instance.ClearAllUIs();
                UIManager.Instance.ShowUI<RoomLobbyUI>();
            });
        }
    }
}
