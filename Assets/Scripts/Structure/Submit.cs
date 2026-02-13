using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace Shurub
{
    public class Submit : Structure
    {
        public override InteractionKind Kind => InteractionKind.Instant;
        protected override string StructureName => "Submit";
        protected override bool IsInteractable => true;
        protected override bool CanInteract(int playerViewId)
        {
            PhotonView pv = PhotonView.Find(playerViewId);
            if (pv == null) return false;

            PlayerController pc = pv.GetComponent<PlayerController>();
            if (pc == null) return false;

            // 아무것도 안 들고 있음
            if (pc.heldIngredient == null)
            {
                Debug.Log("Submit: 빈 손");
                return false;
            }

            // 세트가 아님
            if (pc.heldIngredient.setType == IngredientManager.SetType.Count)
            {
                Debug.Log("Submit: 세트가 아님");
                return false;
            }

            return true;
        }
        protected override void InstantInteract(int playerViewId)
        {
            PhotonView pv = PhotonView.Find(playerViewId);
            if (pv == null) return;

            PlayerController pc = pv.GetComponent<PlayerController>();
            if (pc == null) return;

            if (pc.heldIngredient == null)
            {
                OnInteractionCanceled(playerViewId);
                return;
            }

            int recipeType = (int)pc.heldIngredient.setType;

            // Master에게 제출 요청
            SubmitFunc(recipeType);

            // 시각적/입력 상 성공 처리
            OnInteractionSuccess(playerViewId);
        }
        public void SubmitFunc(int recipeType)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                OrderManager.Instance.ProcessSubmit(recipeType);
            }
            else
            {
                photonView.RPC(
                    nameof(RPC_RequestSubmit),
                    RpcTarget.MasterClient,
                    recipeType
                );
            }
        }

        [PunRPC]
        void RPC_RequestSubmit(int recipeType, PhotonMessageInfo info)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            OrderManager.Instance.ProcessSubmit(recipeType);
        }
        protected override void OnInteractionStart(int playerViewId)
        {
            Debug.Log("Submit Start");
            base.OnInteractionStart(playerViewId);
        }
        [PunRPC]
        protected override void RPC_OnInteractionSuccess(int playerViewId)
        {
            Debug.Log("Submit Complete");
            base.RPC_OnInteractionSuccess(playerViewId);
        }
        [PunRPC]
        protected override void RPC_OnInteractionFailed(int playerViewId)
        {
            Debug.Log("Submit Failed");
            base.RPC_OnInteractionFailed(playerViewId);
        }
    }
}