using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace Shurub
{
    public class Table : Structure
    {
        public override InteractionKind Kind => InteractionKind.Instant;
        protected override string StructureName => "Table";
        protected override bool IsInteractable => true;

        public enum TableState { Blank, Ingredient, Dish }
        protected override bool CanInteract(int playerViewId)
        {
            PlayerController pc = PhotonView.Find(playerViewId)
                                            ?.GetComponent<PlayerController>();
            if (pc.heldIngredient != null) //재료나 접시를 들고있는가?
            {
                //빈 손이 아님
                Debug.Log("빈 손이 아님");
                return false;
            }
            return true;
        }
        protected override void InstantInteract(int playerViewId)
        {
            //접시를 획득함
        }
        protected override void OnInteractionStart(int playerViewId)
        {
            Debug.Log("Table Interaction Start");
            base.OnInteractionStart(playerViewId);
        }
        [PunRPC]
        protected override void RPC_OnInteractionSuccess(int playerViewId)
        {
            Debug.Log("Table Interaction Complete");
            base.RPC_OnInteractionSuccess(playerViewId);
        }
        [PunRPC]
        protected override void RPC_OnInteractionFailed(int playerViewId)
        {
            Debug.Log("Table Interaction Failed");
            base.RPC_OnInteractionFailed(playerViewId);
        }
    }
}