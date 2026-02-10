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
        protected override bool CanInteract()
        {
            PlayerController pc = PhotonView.Find(currentPlayerViewId)
                                            ?.GetComponent<PlayerController>();
            if (pc.heldIngredient != null) //재료나 접시를 들고있는가?
            {
                //빈 손이 아님
                Debug.Log("빈 손이 아님");
                return false;
            }
            return true;
        }
        protected override void InstantInteract()
        {
            //접시를 획득함
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