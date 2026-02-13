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

            if (state != InteractionState.Idle)
            {
                //이미 다른 클라이언트가 상호작용 중
                return;
            }

            if (Kind == InteractionKind.Instant)
            {
                photonView.RPC(
                    nameof(RPC_InstantInteract),
                    RpcTarget.All,
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
            // 검사
            PhotonView playerView = PhotonView.Find(playerViewId);
            if (playerView == null)
                return;

            // 조건 확인
            PlayerController player = playerView.GetComponent<PlayerController>();
            if (!CanInteract(playerViewId))
            {
                EndInteraction(playerViewId);
                return;
            }

            OnInteractionStart(playerViewId);

            InstantInteract(playerViewId);
        }
        protected virtual void InstantInteract(int playerViewId) { }
        [PunRPC]
        protected void RPC_RequestInteract(int playerViewId)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            PhotonView playerView = PhotonView.Find(playerViewId);
            if (playerView == null)
                return;

            if (currentProcess != null || state != InteractionState.Idle)
            {
                EndInteraction(playerViewId);
                return;
            }

            // 조건 확인
            PlayerController player = playerView.GetComponent<PlayerController>();
            if (!CanInteract(playerViewId))
            {
                EndInteraction(playerViewId);
                return;
            }

            OnInteractionStart(playerViewId);

            // BakeProcess 생성
            currentProcess = CreateProcess();

            photonView.RPC(nameof(RPC_StartProcess), RpcTarget.All, playerViewId);

        }
        [PunRPC]
        protected void RPC_StartProcess(int playerViewId)
        {
            if (currentProcess == null)
                currentProcess = CreateProcess();

            currentProcess.StartProcess(playerViewId, this);
        }
        public void InteractProcess(int playerViewId)
        {
            photonView.RPC(
                nameof(RPC_InteractProcess),
                RpcTarget.All,
                playerViewId
            );
        }
        [PunRPC]
        protected void RPC_InteractProcess(int playerViewId)
        {
            if (currentProcess == null)
                return;

            currentProcess.InteractProcess(playerViewId);
        }
        [PunRPC]
        public void RPC_SendTimingResult(int playerViewId, bool success)
        {
            if (currentProcess == null) return;
            ((BakeProcess)currentProcess).ApplyTimingResult(playerViewId, success);
        }
        public void RequestCancel(int playerViewId)
        {
            photonView.RPC(
                nameof(RPC_RequestCancel),
                RpcTarget.All,
                playerViewId
            );
        }
        [PunRPC]
        protected void RPC_RequestCancel(int playerViewId)
        {
            if (currentProcess == null) return;

            currentProcess.CanceledProcess(playerViewId);
        }
        protected abstract bool CanInteract(int playerViewId);
        protected virtual InteractionProcess CreateProcess() { return null; }
        public void ClearProcess()
        {
            currentProcess = null;
        }
        public void UpdateProgress(int playerViewId, float _progress, bool active)
        {
            photonView.RPC(
                nameof(RPC_UpdateProgress),
                RpcTarget.All,
                playerViewId,
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
            if(Kind == InteractionKind.Process)
            {
                photonView.RPC(
                    nameof(RPC_SyncState),
                    RpcTarget.All,
                    (int)InteractionState.InProgress
                );
                UpdateProgress(playerViewId, 0f, true);
            }
        }
        public virtual void OnInteractionSuccess(int playerViewId)
        {
            photonView.RPC(
                nameof(RPC_OnInteractionSuccess),
                RpcTarget.All,
                playerViewId
            );

            EndInteraction(playerViewId);
        }
        [PunRPC]
        protected virtual void RPC_OnInteractionSuccess(int playerViewId)
        {
            //animator.SetTrigger("Success");
            //successEffect.Play();
        }
        public virtual void OnInteractionFailed(int playerViewId)
        {
            photonView.RPC(
                nameof(RPC_OnInteractionFailed),
                RpcTarget.All,
                playerViewId
            );

            EndInteraction(playerViewId);
        }
        [PunRPC]
        protected virtual void RPC_OnInteractionFailed(int playerViewId)
        {
            //animator.SetTrigger("Failed");
            //failedEffect.Play();
        }
        public virtual void OnInteractionCanceled(int playerViewId)
        {
            photonView.RPC(
                nameof(RPC_OnInteractionCanceled),
                RpcTarget.All,
                playerViewId
            );

            EndInteraction(playerViewId);
        }
        [PunRPC]
        protected virtual void RPC_OnInteractionCanceled(int playerViewId)
        {
            //animator.SetTrigger("Canceled");
            //canceledEffect.Play();
        }
        protected virtual void EndInteraction(int playerViewId)
        {
            if (Kind == InteractionKind.Process)
            {
                photonView.RPC(
                    nameof(RPC_SyncState),
                    RpcTarget.All,
                    (int)InteractionState.Idle
                );
                UpdateProgress(playerViewId, 0, false);
            }
            photonView.RPC(
                nameof(RPC_EndInteraction),
                RpcTarget.All,
                playerViewId
            );
        }
        [PunRPC]
        protected virtual void RPC_EndInteraction(int playerViewId)
        {
            PlayerController pc = PhotonView.Find(playerViewId)
                                            ?.GetComponent<PlayerController>();

            if (pc != null)
            {
                Debug.Log("pc.currentInteractable = null");
                pc.currentInteractable = null;
            }
            
            currentProcess = null;
        }
    }
}