using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace Shurub
{
    public class Cutting : Structure
    {
        [SerializeField]
        private CutProcess processPrefab;

        public override float Progress => progress; // 0~1
        private float progress = 0f;
        private const float PROCESS_INTERVAL = 1f / 6f;
        public override InteractionKind Kind => InteractionKind.Process;
        protected override string StructureName => "Cutting";
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
            if (!pc.heldIngredient.IsCuttable) //이 재료는 썰 수 있는가?
            {
                //이 재료는 썰 수 없음
                Debug.Log("이 재료는 썰 수 없음");
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
            Debug.Log("Cut Start");
            base.OnInteractionStart(playerViewId);
        }
        protected override void OnInteractionSuccess(int playerViewId)
        {
            Debug.Log("Cut Complete");
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
        public void OnInteractInput()
        {
            if (!photonView.IsMine) return;

            if (state != InteractionState.InProgress)
                return;
            
            AddProgress(PROCESS_INTERVAL);
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
    }
}