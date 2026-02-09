using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace Shurub
{
    public class Stove : Structure
    {
        [SerializeField]
        private BakeProcess processPrefab;

        public override float Progress => progress; // 0~1
        private float progress = 0f;
        private const float PROCESS_INTERVAL = 1f;
        private const int FAILED_LIMIT = 3;
        private int failedCount = 0;

        public override InteractionKind Kind => InteractionKind.Process;
        protected override string StructureName => "Stove";
        protected override bool IsInteractable => true;

        protected override bool CanInteract()
        {
            if (currPC.heldIngredient == null) //재료를 들고있는가?
            {
                //재료를 들고 있지 않음
                Debug.Log("재료를 들고 있지 않음");
                return false;
            }
            if (!currPC.heldIngredient.IsBakable) //재료를 구울 수 있는가?
            {
                //이 재료는 구울 수 없음
                Debug.Log("이 재료는 구울 수 없음");
                return false;
            }
            if (currPC.heldIngredient.state != Ingredient.IngredientState.unCooked) //이 재료는 조리되지 않은 상태인가?
            {
                //이 재료는 이미 조리되었거나 탄 재료임
                Debug.Log("이 재료는 이미 조리되었거나 탄 재료임");
                return false;
            }
            return true;
        }
        protected override InteractionProcess CreateProcess()
        {
            return Instantiate(processPrefab, transform);
        }
        protected override void OnInteractionStart(int playerViewId)
        {
            Debug.Log("Bake Start");
            base.OnInteractionStart(playerViewId);
            progress = 0f;
            //재료를 안보이게 비활성화 함
            currPC.heldIngredient.SetActive(false);
        }

        public override void OnInteractionSuccess()
        {
            Debug.Log("Bake Complete");
            //재료 state 바꾸면서 스프라이트도 변경
            //다시 재료가 보이게 됨
            currPC.heldIngredient.OnCooked();
            currPC.heldIngredient.SetActive(true);
            base.OnInteractionSuccess();
        }
        public override void OnInteractionFailed()
        {
            Debug.Log("Bake Canceled");
            //재료 state 바꾸면서 스프라이트도 변경
            //다시 재료가 보이게 됨
            currPC.heldIngredient.OnBurned();
            currPC.heldIngredient.SetActive(true);
            base.OnInteractionFailed();
        }
        public override void OnInteractionCanceled()
        {
            Debug.Log("Bake Canceled");

            //다시 재료가 보이게 됨
            currPC.heldIngredient.SetActive(true);
            base.OnInteractionCanceled();
        }
        [PunRPC]
        protected override void RPC_OnInteractionSuccess(int playerViewId)
        {
            base.RPC_OnInteractionSuccess(playerViewId);
        }
        [PunRPC]
        protected override void RPC_OnInteractionFailed(int playerViewId)
        {
            base.RPC_OnInteractionFailed(playerViewId);
        }
        [PunRPC]
        protected override void RPC_OnInteractionCanceled(int playerViewId)
        {
            base.RPC_OnInteractionCanceled(playerViewId);
        }
    }
}