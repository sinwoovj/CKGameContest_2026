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

        // Use this for initialization
        protected override void Start()
        {
            base.Start();

        }

        // Update is called once per frame
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
            Debug.Log("Interaction Start");
            base.OnInteractionStart(playerViewId);
        }
        protected override void OnInteractionSuccess(int playerViewId)
        {
            Debug.Log("Interaction Complete");
            base.OnInteractionSuccess(playerViewId);
        }
    }
}