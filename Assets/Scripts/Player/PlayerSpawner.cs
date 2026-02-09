using Cysharp.Threading.Tasks;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using System.Collections.Generic;
using UnityEngine;

namespace Shurub
{
    public class PlayerSpawner : Singleton<PlayerSpawner>
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private List<GameObject> playerSpawnPointObject;

        private bool isSpawned = false;

        protected override bool CheckDontDestroyOnLoad()
        {
            return false;
        }

        private void Start()
        {
            TrySpawnPlayer();
        }

        private void TrySpawnPlayer()
        {
            if (!PhotonNetwork.InRoom)
                return;

            if (PhotonNetwork.LocalPlayer.TagObject != null)
                return;

            int myPNum = playerSpawnPointObject.Count - 1;

            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("pNum", out object value))
            {
                myPNum = (int)value;
                Debug.Log("MyPNum : " +  myPNum);
            }

            GameObject player = PhotonNetwork.Instantiate(
                "Prefabs/Player/" + playerPrefab.name,
                playerSpawnPointObject[myPNum].transform.position,
                Quaternion.identity
            );

            PhotonNetwork.LocalPlayer.TagObject = player;
            GameManager.Instance.RegisterPlayer(player.GetComponent<Player>());

            isSpawned = true;
        }

        public void ReSpawnPlayer()
        {
            ProcessRespawnPlayer().Forget();
        }

        private async UniTaskVoid ProcessRespawnPlayer()
        {
            if (!PhotonNetwork.InRoom)
                return;

            await UniTask.WaitUntil(() => isSpawned, cancellationToken: this.GetCancellationTokenOnDestroy());

            int myPNum = 0;

            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("pNum", out object value))
            {
                myPNum = (int)value;
                Debug.Log("MyPNum : " + myPNum);
            }
            else
            {
                Debug.LogError("pNum could not load");
            }

            GameManager.Instance.LocalPlayer.transform.position = playerSpawnPointObject[myPNum].transform.position;
        }
    }
}
