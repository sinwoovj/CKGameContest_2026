using Photon.Pun;
using UnityEngine;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerPrefab;

    void Start()
    {
        TrySpawn();
    }

    public override void OnJoinedRoom()
    {
        TrySpawn();
    }

    private void TrySpawn()
    {
        if (!PhotonNetwork.InRoom)
            return;

        if (PhotonNetwork.LocalPlayer.TagObject != null)
            return;

        GameObject player = PhotonNetwork.Instantiate(
            "Prefabs/" + playerPrefab.name,
            Vector3.zero,
            Quaternion.identity
        );

        PhotonNetwork.LocalPlayer.TagObject = player;
    }
}