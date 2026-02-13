using Photon.Pun;
using UnityEngine;

namespace Shurub
{
    public class Table : Structure
    {
        public override InteractionKind Kind => InteractionKind.Instant;
        protected override string StructureName => "Table";
        protected override bool IsInteractable => true;
        public Ingredient setIngredient;

        private enum TableAction
        {
            None,
            PlaceItem,
            AddToPlate,
            TakeItem,
            TakePlate,
            TakePlateIngredient,
            CookPlate
        }
        protected override bool CanInteract(int playerViewId)
        {
            return DecideAction(GetPlayer(playerViewId)) != TableAction.None;
        }

        protected override void InstantInteract(int playerViewId)
        {
            PhotonView playerPV = PhotonView.Find(playerViewId);
            if (playerPV == null || !playerPV.IsMine) return;

            PlayerController pc = playerPV.GetComponent<PlayerController>();
            if (pc == null) return;

            TableAction action = DecideAction(pc);

            switch (action)
            {
                case TableAction.PlaceItem:
                    photonView.RPC(nameof(RPC_PlaceItem), RpcTarget.All, playerViewId);
                    break;

                case TableAction.AddToPlate:
                    photonView.RPC(nameof(RPC_AddToPlate), RpcTarget.All, playerViewId);
                    break;

                case TableAction.TakeItem:
                    photonView.RPC(nameof(RPC_TakeItem), RpcTarget.All, playerViewId);
                    break;

                case TableAction.TakePlate:
                    photonView.RPC(nameof(RPC_TakePlate), RpcTarget.All, playerViewId);
                    break;

                case TableAction.TakePlateIngredient:
                    photonView.RPC(nameof(RPC_TakePlateIngredient), RpcTarget.All, playerViewId);
                    break;

                default:
                    OnInteractionFailed(playerViewId);
                    break;
            }
        }

        private TableAction DecideAction(PlayerController pc)
        {
            bool playerHasItem = pc.heldIngredient != null;
            bool tableEmpty = setIngredient == null;

            if (playerHasItem)
            {
                Ingredient held = pc.heldIngredient;

                if (tableEmpty)
                    return TableAction.PlaceItem;

                // 테이블에 무언가 있음
                if (IsPlate(setIngredient))
                {
                    // 플레이어가 그릇 또는 세트 들고 있으면 실패
                    if (IsPlate(held) || held.IsSet())
                        return TableAction.None;

                    // 조리 안된 재료면 실패
                    if (!held.IsCooked())
                        return TableAction.None;

                    return TableAction.AddToPlate;
                }

                // 테이블에 재료나 세트가 있으면 실패
                return TableAction.None;
            }
            else
            {
                // 플레이어 빈손

                if (tableEmpty)
                    return TableAction.None;

                if (IsPlate(setIngredient))
                {
                    Plate plate = setIngredient as Plate;

                    if (plate.HasIngredient())
                        return TableAction.TakePlateIngredient;
                    else
                        return TableAction.TakePlate;
                }

                return TableAction.TakeItem;
            }
        }
        protected override bool CanHoldInteract(int playerViewId)
        {
            return DecideHoldAction(GetPlayer(playerViewId)) == TableAction.CookPlate;
        }
        protected override void HoldInstantInteract(int playerViewId)
        {
            PhotonView playerPV = PhotonView.Find(playerViewId);
            if (playerPV == null || !playerPV.IsMine) return;

            PlayerController pc = playerPV.GetComponent<PlayerController>();
            if (pc == null) return;

            TableAction action = DecideHoldAction(pc);

            if (action == TableAction.CookPlate)
            {
                photonView.RPC(
                    nameof(RPC_CookPlate),
                    RpcTarget.MasterClient,
                    playerViewId
                );
            }
        }

        private TableAction DecideHoldAction(PlayerController pc)
        {
            if (setIngredient == null)
                return TableAction.None;

            if (!IsPlate(setIngredient))
                return TableAction.None;

            Plate plate = setIngredient as Plate;

            if (!plate.HasIngredient())
                return TableAction.None;

            // 요리 가능한지 체크
            if (!plate.CanCook())
                return TableAction.None;

            return TableAction.CookPlate;
        }
        [PunRPC]
        private void RPC_CookPlate(int playerViewId)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            PlayerController pc = GetPlayer(playerViewId);
            if (pc == null) return;

            Plate plate = setIngredient as Plate;
            if (plate == null) return;

            int resultSetType = plate.TryCook();
            if (resultSetType < 0)
            {
                OnInteractionFailed(playerViewId);
                return;
            }

            // 요리 생성
            GameObject setObj =
                IngredientManager.Instance.InstantiateSet(
                    (IngredientManager.SetType)resultSetType,
                    transform.position
                );

            if (setObj == null) return;

            // 기존 재료 제거
            plate.ClearIngredients(playerViewId);
            // 기존 접시 제거
            IngredientManager.Instance.DestroyIngredient(setIngredient);
            setIngredient = null;

            // 플레이어에게 지급
            photonView.RPC(
                nameof(RPC_GiveCookedSet),
                pc.photonView.Owner,
                playerViewId,
                setObj.GetPhotonView().ViewID
            );
        }
        [PunRPC]
        private void RPC_GiveCookedSet(int playerViewId, int ingredientViewId)
        {
            PlayerController pc = GetPlayer(playerViewId);
            if (pc == null) return;

            pc.GetIngredient(ingredientViewId);

            OnInteractionSuccess(playerViewId);
        }

        [PunRPC]
        private void RPC_PlaceItem(int playerViewId)
        {
            PlayerController pc = GetPlayer(playerViewId);
            if (pc == null || pc.heldIngredient == null) return;

            setIngredient = pc.heldIngredient;
            pc.DropIngredient((Vector2)transform.position);

            setIngredient.SetOnTable(transform);

            OnInteractionSuccess(playerViewId);
        }

        [PunRPC]
        private void RPC_AddToPlate(int playerViewId)
        {
            PlayerController pc = GetPlayer(playerViewId);
            if (pc == null || pc.heldIngredient == null) return;

            Plate plate = setIngredient as Plate;
            if (plate == null) return;

            int ingredientViewId = pc.heldIngredient.photonView.ViewID;

            bool success = plate.TryAddIngredient(
                ingredientViewId,
                (int)pc.heldIngredient.ingredientType,
                playerViewId
            );

            if (!success)
            {
                OnInteractionFailed(playerViewId);
                return;
            }

            pc.heldIngredient.SetScale(0.7f);
            pc.heldIngredient.ChangeSpriteSortingOrder(3);
            pc.DropIngredient((Vector2)transform.position);

            OnInteractionSuccess(playerViewId);
        }

        [PunRPC]
        private void RPC_TakeItem(int playerViewId)
        {
            PlayerController pc = GetPlayer(playerViewId);
            if (pc == null || setIngredient == null) return;
            setIngredient.UnSetOnTable();
            pc.GetIngredient(setIngredient.photonView.ViewID);
            setIngredient = null;

            OnInteractionSuccess(playerViewId);
        }

        [PunRPC]
        private void RPC_TakePlate(int playerViewId)
        {
            PlayerController pc = GetPlayer(playerViewId);
            if (pc == null || setIngredient == null) return;

            setIngredient.UnSetOnTable();
            pc.GetIngredient(setIngredient.photonView.ViewID);
            setIngredient = null;

            OnInteractionSuccess(playerViewId);
        }

        [PunRPC]
        private void RPC_TakePlateIngredient(int playerViewId)
        {
            PlayerController pc = GetPlayer(playerViewId);
            if (pc == null) return;

            Plate plate = setIngredient as Plate;
            if (plate == null) return;

            int ingredientViewId;
            bool success = plate.TrySubtractIngredient(out ingredientViewId, playerViewId);

            if (!success)
            {
                OnInteractionFailed(playerViewId);
                return;
            }

            pc.GetIngredient(ingredientViewId);
            pc.heldIngredient.ChangeSpriteSortingOrder(2);
            pc.heldIngredient.SetScale(1f * (1 / pc.transform.localScale.x));

            OnInteractionSuccess(playerViewId);
        }

        private PlayerController GetPlayer(int viewId)
        {
            PhotonView pv = PhotonView.Find(viewId);
            if (pv == null) return null;
            return pv.GetComponent<PlayerController>();
        }

        private bool IsPlate(Ingredient ingredient)
        {
            return ingredient.ingredientType == IngredientManager.IngredientType.Plate;
        }

    }
}
