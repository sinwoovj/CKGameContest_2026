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

        protected override void Start()
        {
            base.Start();

        }

        protected override void Update()
        {
            base.Update();

        }
        protected override bool CanInteract(PlayerController pc)
        {
            if (pc.heldIngredient == null) //재료를 들고있는가?
            {
                //재료를 들고 있지 않음
                Debug.Log("재료를 들고 있지 않음");
                return false;
            }
            return true;
        }
        protected override void InstantInteract(PlayerController pc)
        {
            pc.RemoveIngredient();
            OnInteractionSuccess(currentPlayerViewId);
        }
        protected override void OnInteractionStart(int playerViewId)
        {
            Debug.Log("Throw Out Start");
            base.OnInteractionStart(playerViewId);
        }
        protected override void OnInteractionSuccess(int playerViewId)
        {
            Debug.Log("Throw Out Complete");
            base.OnInteractionSuccess(playerViewId);
        }
    }
}