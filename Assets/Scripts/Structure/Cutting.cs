using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace Shurub
{
    public class Cutting : Structure
    {
        [SerializeField]
        private CutProcess processPrefab;
        public override InteractionKind Kind => InteractionKind.Process;
        protected override string StructureName => "Cutting";
        protected override bool IsInteractable => true;

        protected override bool CanInteract(int playerViewId)
        {
            PlayerController pc = PhotonView.Find(playerViewId)
                                            ?.GetComponent<PlayerController>();
            if (pc.heldIngredient == null) //재료를 들고있는가?
            {
                //재료를 들고 있지 않음
                Debug.Log("재료를 들고 있지 않음");
                return false;
            }
            if (!pc.heldIngredient.IsCuttable) //이 재료는 썰 수 있는가?
            {
                //이 재료는 썰 수 없음
                Debug.Log("이 재료는 썰 수 없음");
                return false;
            }
            if (pc.heldIngredient.state != Ingredient.IngredientState.unCooked) //이 재료는 조리되지 않은 상태인가?
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
            PlayerController pc = PhotonView.Find(playerViewId)
                                            ?.GetComponent<PlayerController>();
            if (pc == null) return;
            Debug.Log("Cut Start");
            base.OnInteractionStart(playerViewId);
            //재료를 안보이게 비활성화 함
            pc.heldIngredient.SetActive(false);
        }
        public override void OnInteractionSuccess(int playerViewId)
        {
            PlayerController pc = PhotonView.Find(playerViewId)
                                            ?.GetComponent<PlayerController>();
            if (pc == null) return;
            Debug.Log("Cut Complete");
            //재료 state 바꾸면서 스프라이트도 변경
            //다시 재료가 보이게 됨
            pc.heldIngredient.OnCooked();
            pc.heldIngredient.SetActive(true);
            base.OnInteractionSuccess(playerViewId);
        }
        public override void OnInteractionCanceled(int playerViewId)
        {
            PlayerController pc = PhotonView.Find(playerViewId)
                                            ?.GetComponent<PlayerController>();
            if (pc == null) return;
            Debug.Log("Cut Canceled");

            //다시 재료가 보이게 됨
            pc.heldIngredient.SetActive(true);
            base.OnInteractionCanceled(playerViewId);
        }
    }
}