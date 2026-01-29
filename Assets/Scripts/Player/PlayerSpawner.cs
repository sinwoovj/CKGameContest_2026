using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using System.Collections.Generic;
using UnityEngine;

namespace Shurub
{
    public class PlayerSpawner : MonoBehaviourPunCallbacks
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private List<GameObject> playerSpawnPointObject;

        static public PlayerSpawner Instance;

        private void Start()
        {
            Instance = this;
            TrySpawnPlayer();
        }

        public override void OnJoinedRoom()
        {
            TrySpawnPlayer();
        }

        public void TrySpawnPlayer()
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
                "Prefabs/" + playerPrefab.name,
                playerSpawnPointObject[myPNum].transform.position,
                Quaternion.identity
            );

            PhotonNetwork.LocalPlayer.TagObject = player;
        }

        public void ReSpawnPlayer()
        {
            if (!PhotonNetwork.InRoom)
                return;

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
            
            PlayerManager.LocalPlayerInstance.transform.position = playerSpawnPointObject[myPNum].transform.position;
        }
    }
}
