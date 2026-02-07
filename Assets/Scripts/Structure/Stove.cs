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
            if (!pc.heldIngredient.IsBakable) //재료를 구울 수 있는가?
            {
                //이 재료는 구울 수 없음
                Debug.Log("이 재료는 구울 수 없음");
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
        }
        protected override void OnInteractionSuccess(int playerViewId)
        {
            Debug.Log("Bake Complete");
            base.OnInteractionSuccess(playerViewId);
        }

        [PunRPC]
        protected override void RPC_SyncState(int newState)
        {
            base.RPC_SyncState(newState);
            InteractionUI.Instance.UpdateUI(this);
        }


        public void CancelInteraction()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            OnInteractionCanceled(currentPlayerViewId);
        }

        public void OnInteractInput(bool isSuccess)
        {
            if (!photonView.IsMine) return;

            if (state != InteractionState.InProgress)
                return;

            if (isSuccess)
                AddProgress(PROCESS_INTERVAL);
            else
                FailOnce();
        }
        protected void AddProgress(float amount)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            progress = Mathf.Clamp01(progress + amount);

            photonView.RPC(
                nameof(RPC_SyncState),
                RpcTarget.All,
                (int)state
            );

            if (progress >= 1f)
            {
                OnInteractionSuccess(currentPlayerViewId);
            }
        }

        protected void FailOnce()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            if (++failedCount >= FAILED_LIMIT)
            {
                OnInteractionFailed(currentPlayerViewId);
            }
        }
    }
}