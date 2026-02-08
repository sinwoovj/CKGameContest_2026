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
        protected override bool CanInteract()
        {
            if (currPC.heldIngredient == null) //음식이 담긴 접시를 들고있는가?
            {
                //빈 손임
                Debug.Log("빈 손임");
                return false;
            }
            return true;
        }
        protected override void InstantInteract()
        {
            //오더리스트에 해당하는 음식을 들고 있는가?

            //성공적으로 음식 제출 시...
            OnInteractionSuccess();
            //아니라면 Failed 호출
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