using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace Shurub
{
    public class Trash : Structure
    {
        public override InteractionKind Kind => InteractionKind.Instant;
        protected override string StructureName => "Trash";
        protected override bool IsInteractable => true;
        protected override bool CanInteract()
        {
            if (currPC.heldIngredient == null) //재료를 들고있는가?
            {
                //재료를 들고 있지 않음
                Debug.Log("재료를 들고 있지 않음");
                return false;
            }
            return true;
        }
        protected override void InstantInteract()
        {
            currPC.RemoveIngredient();
            OnInteractionSuccess();
        }
        [PunRPC]
        protected override void OnInteractionStart(int playerViewId)
        {
            Debug.Log("Throw Out Start");
            base.OnInteractionStart(playerViewId);
        }
        [PunRPC]
        protected override void RPC_OnInteractionSuccess(int playerViewId)
        {
            Debug.Log("Throw Out Complete");
            base.RPC_OnInteractionSuccess(playerViewId);
        }
        [PunRPC]
        protected override void RPC_OnInteractionFailed(int playerViewId)
        {
            Debug.Log("Throw Out Failed");
            base.RPC_OnInteractionFailed(playerViewId);
        }
    }
}