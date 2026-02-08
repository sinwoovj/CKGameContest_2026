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
        public virtual float Progress => 0f;

        public InteractionState state = InteractionState.Idle;
        protected int currentPlayerViewId;
        protected InteractionProcess currentProcess;
        protected PlayerController currPC;

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
        public void UpdateProcess(float deltaTime)
        {
            photonView.RPC(
                nameof(RPC_UpdateProcess),
                RpcTarget.MasterClient,
                deltaTime
            );
        }
        [PunRPC]
        protected void RPC_UpdateProcess(float deltaTime)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;
            currentProcess.UpdateProcess(deltaTime);
        }
        public void InteractProcess()
        {
            photonView.RPC(
                nameof(RPC_InteractProcess),
                RpcTarget.MasterClient
            );
        }
        [PunRPC]
        protected void RPC_InteractProcess()
        {
            if (!PhotonNetwork.IsMasterClient)
                return;
            currentProcess.InteractProcess();
        }

        public void ClearProcess()
        {
            currentProcess = null;
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

            currPC = PhotonView.Find(playerViewId).GetComponent<PlayerController>();

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

            currPC = PhotonView.Find(playerViewId).GetComponent<PlayerController>();

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

        [PunRPC]
        protected void RPC_SyncState(int newState)
        {
            state = (InteractionState)newState;
        }
        protected abstract bool CanInteract();
        protected virtual InteractionProcess CreateProcess() { return null; }
        protected virtual void OnInteractionStart(int playerViewId) // 애니메이션·이펙트 전환용 함수
        {
            // State 동기화 및 UI 동기화
            state = InteractionState.InProgress;
            PhotonView.Find(playerViewId).GetComponent<PlayerController>().interactionState = state;

            photonView.RPC(
                nameof(RPC_SyncState),
                RpcTarget.All,
                (int)state
            );
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
            state = InteractionState.Idle;
            currPC.interactionState = state;
            currPC.currentInteractable = null;
            photonView.RPC(
                nameof(RPC_SyncState),
                RpcTarget.All,
                (int)state
            );
            currentPlayerViewId = 0;
            currPC = null;
        }

        public virtual void UpdateProgress(float _progress) { }
    }
}