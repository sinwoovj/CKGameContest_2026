using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace Shurub
{
    public class Pantry : Structure
    {
        public override InteractionKind Kind => InteractionKind.Instant;
        protected override string StructureName => "Pantry";
        protected override bool IsInteractable => true;
        public GameObject platePrefab;
        protected override bool CanInteract(int playerViewId)
        {
            PlayerController pc = PhotonView.Find(playerViewId)
                                            ?.GetComponent<PlayerController>();
            if (pc.heldIngredient != null) //빈 손이 아닌가?
            {
                Debug.Log("빈 손이 아님");
                return false;
            }
            return true;
        }
        protected override void InstantInteract(int playerViewId)
        {
            PhotonView playerPV = PhotonView.Find(playerViewId);
            if (playerPV == null) return;

            if (!playerPV.IsMine) return;
            PlayerController pc = playerPV.GetComponent<PlayerController>();
            if (pc == null) return;

            photonView.RPC(
                nameof(RPC_RequestPlateSpawn),
               RpcTarget.MasterClient,
               playerViewId
            );
            OnInteractionSuccess(playerViewId);
        }
        [PunRPC]
        private void RPC_RequestPlateSpawn(int playerViewId)
        {
            Debug.Log("Spawn Plate by Master");

            PhotonView playerPV = PhotonView.Find(playerViewId);
            if (playerPV == null) return;

            PlayerController pc = playerPV.GetComponent<PlayerController>();
            if (pc == null) return;

            GameObject plateObj =
                IngredientManager.Instance.InstantiateIngredient(
                    IngredientManager.IngredientType.Plate,
                    pc.transform.position
                );

            if (plateObj == null) return;

            photonView.RPC(
                nameof(RPC_RequestGetPlate),
               playerPV.Owner,
               playerViewId,
               plateObj.GetPhotonView().ViewID
            );
        }
        [PunRPC]
        private void RPC_RequestGetPlate(int playerViewId, int plateViewId)
        {
            PhotonView playerPV = PhotonView.Find(playerViewId);
            if (playerPV == null) return;

            PlayerController pc = playerPV.GetComponent<PlayerController>();
            if (pc == null) return;

            pc.GetPlate(plateViewId);
        }
        protected override void OnInteractionStart(int playerViewId)
        {
            Debug.Log("Get Dish Start");
            base.OnInteractionStart(playerViewId);
        }
        [PunRPC]
        protected override void RPC_OnInteractionSuccess(int playerViewId)
        {
            Debug.Log("Get Dish Complete");
            base.RPC_OnInteractionSuccess(playerViewId);
        }
        [PunRPC]
        protected override void RPC_OnInteractionFailed(int playerViewId)
        {
            Debug.Log("Get Dish Failed");
            base.RPC_OnInteractionFailed(playerViewId);
        }
    }
}