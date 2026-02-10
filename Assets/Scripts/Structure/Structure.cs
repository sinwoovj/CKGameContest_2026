using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace Shurub
{
    public abstract class Structure : MonoBehaviourPun, IInteractable
    {
        public virtual InteractionKind Kind => InteractionKind.Process;
        protected virtual string StructureName => "Structure";
        protected virtual bool IsInteractable => false;
        public virtual float Progress => progress;
        private float progress = 0f;
        public GameObject progressUIPrefab;
        public InteractionState state = InteractionState.Idle;
        protected int currentPlayerViewId;
        protected InteractionProcess currentProcess;

        // Reference Components
        protected Rigidbody2D rb;
        protected Collider2D col;

        // Monohaviour Functions
        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
        }
        public void Interact(PlayerController player)
        {
            // 로컬 플레이어만 요청
            if (!player.photonView.IsMine) return;

            if (photonView.ViewID == 0)
            {
                PhotonNetwork.AllocateViewID(photonView);
            }

            if (state != InteractionState.Idle)
            {
                //이미 다른 클라이언트가 상호작용 중
                return;
            }

            if (Kind == InteractionKind.Instant)
            {
                photonView.RPC(
                    nameof(RPC_InstantInteract),
                    RpcTarget.MasterClient,
                    player.photonView.ViewID
                );
            }
            else
            {
                photonView.RPC(
                    nameof(RPC_RequestInteract),
                    RpcTarget.MasterClient,
                    player.photonView.ViewID
                );
            }
        }
        [PunRPC]
        protected void RPC_InstantInteract(int playerViewId)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            // 검사
            PhotonView playerView = PhotonView.Find(playerViewId);
            if (playerView == null)
                return;

            currentPlayerViewId = playerViewId;

            // 조건 확인
            PlayerController player = playerView.GetComponent<PlayerController>();
            if (!CanInteract())
            {
                EndInteraction();
                return;
            }

            OnInteractionStart(playerViewId);

            InstantInteract();
        }
        protected virtual void InstantInteract() { }
        [PunRPC]
        protected void RPC_RequestInteract(int playerViewId)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            PhotonView playerView = PhotonView.Find(playerViewId);
            if (playerView == null)
                return;

            currentPlayerViewId = playerViewId;

            if (currentProcess != null || state != InteractionState.Idle)
            {
                EndInteraction();
                return;
            }


            // 조건 확인
            PlayerController player = playerView.GetComponent<PlayerController>();
            if (!CanInteract())
            {
                EndInteraction();
                return;
            }

            OnInteractionStart(playerViewId);

            // BakeProcess 생성
            currentProcess = CreateProcess();
            currentProcess.StartProcess(playerViewId, this);
        }
        public void InteractProcess(int playerViewId)
        {
            photonView.RPC(
                nameof(RPC_InteractProcess),
                RpcTarget.MasterClient,
                playerViewId
            );
        }
        [PunRPC]
        protected void RPC_InteractProcess(int playerViewId)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            if (currentProcess == null)
                return;

            if (currentPlayerViewId != playerViewId)
                return; // 다른 사람이 실행 못 함

            currentProcess.InteractProcess();
        }
        public void RequestCancel(int playerViewId)
        {
            photonView.RPC(
                nameof(RPC_RequestCancel),
                RpcTarget.MasterClient,
                playerViewId
            );
        }
        [PunRPC]
        protected void RPC_RequestCancel(int playerViewId)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            if (currentProcess == null) return;
            if (currentPlayerViewId != playerViewId) return; // 다른 사람이 취소 못 함

            currentProcess.CanceledProcess();
        }
        protected abstract bool CanInteract();
        protected virtual InteractionProcess CreateProcess() { return null; }
        public void ClearProcess()
        {
            currentProcess = null;
        }
        public void UpdateProgress(float _progress, bool active)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            photonView.RPC(
                nameof(RPC_UpdateProgress),
                RpcTarget.All,
                currentPlayerViewId,
                _progress,
                photonView.ViewID,
                active
            );
        }
        [PunRPC]
        protected void RPC_UpdateProgress(int playerViewId, float _progress, int structureViewId, bool active)
        {
            progress = _progress;
            PlayerController pc = PhotonView.Find(playerViewId)
                                            ?.GetComponent<PlayerController>();
            if (pc == null) return;

            pc.currentInteractable =
                structureViewId == 0
                ? null
                : PhotonView.Find(structureViewId)
                      ?.GetComponent<IInteractable>();
            pc.EnsureProcessUI();   // 있으면 패스, 없으면 생성
            pc.progressUI.SetProgress(progress);
            pc.progressUI.SetActive(active);
        }
        [PunRPC]
        protected void RPC_SyncState(int newState)
        {
            state = (InteractionState)newState;
        }
        protected virtual void OnInteractionStart(int playerViewId) // 애니메이션·이펙트 전환용 함수
        {
            // State 동기화 및 UI 동기화
            photonView.RPC(
                nameof(RPC_SyncState),
                RpcTarget.All,
                (int)InteractionState.InProgress
            );
            UpdateProgress(0f, true);
        }
        public virtual void OnInteractionSuccess()
        {
            photonView.RPC(
                nameof(RPC_OnInteractionSuccess),
                RpcTarget.All,
                currentPlayerViewId
            );

            EndInteraction();
        }
        [PunRPC]
        protected virtual void RPC_OnInteractionSuccess(int playerViewId)
        {
            //animator.SetTrigger("Success");
            //successEffect.Play();
        }
        public virtual void OnInteractionFailed()
        {
            photonView.RPC(
                nameof(RPC_OnInteractionFailed),
                RpcTarget.All,
                currentPlayerViewId
            );

            EndInteraction();
        }
        [PunRPC]
        protected virtual void RPC_OnInteractionFailed(int playerViewId)
        {
            //animator.SetTrigger("Failed");
            //failedEffect.Play();
        }
        public virtual void OnInteractionCanceled()
        {
            photonView.RPC(
                nameof(RPC_OnInteractionCanceled),
                RpcTarget.All,
                currentPlayerViewId
            );

            EndInteraction();
        }
        [PunRPC]
        protected virtual void RPC_OnInteractionCanceled(int playerViewId)
        {
            //animator.SetTrigger("Canceled");
            //canceledEffect.Play();
        }
        protected virtual void EndInteraction()
        {
            photonView.RPC(
                nameof(RPC_SyncState),
                RpcTarget.All,
                (int)InteractionState.Idle
            );
            UpdateProgress(0, false);
            photonView.RPC(
                nameof(RPC_EndInteraction),
                RpcTarget.All,
                currentPlayerViewId
            );
        }
        [PunRPC]
        protected virtual void RPC_EndInteraction(int playerViewId)
        {
            PlayerController pc = PhotonView.Find(playerViewId)
                                            ?.GetComponent<PlayerController>();
            pc.currentInteractable = null;
            currentPlayerViewId = 0;
        }
    }
}