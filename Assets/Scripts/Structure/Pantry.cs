using System.Collections;
using UnityEngine;

namespace Shurub
{
    public class Pantry : Structure
    {
        public override InteractionKind Kind => InteractionKind.Instant;
        protected override string StructureName => "Pantry";
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
            if (pc.heldIngredient != null) //재료나 접시를 들고있는가?
            {
                //빈 손이 아님
                Debug.Log("빈 손이 아님");
                return false;
            }
            return true;
        }
        protected override void InstantInteract(PlayerController pc)
        {
            //접시를 획득함
        }
        protected override void OnInteractionStart(int playerViewId)
        {
            Debug.Log("Get Dish Start");
            base.OnInteractionStart(playerViewId);
        }
        protected override void OnInteractionSuccess(int playerViewId)
        {
            Debug.Log("Get Dish Complete");
            base.OnInteractionSuccess(playerViewId);
        }
    }
}