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

        // Reference Components
        protected Rigidbody2D rb;
        protected Collider2D col;

        // Monohaviour Functions
        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
        }

        protected virtual void Start()
        {

        }

        protected virtual void Update()
        {

        }

        public void Interact(PlayerController player)
        {
            // 로컬 플레이어만 요청
            if (!player.photonView.IsMine) return;

            if (photonView.ViewID == 0)
            {
                PhotonNetwork.AllocateViewID(photonView);
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

            // 조건 확인
            PlayerController player = playerView.GetComponent<PlayerController>();
            if (!CanInteract(player))
            {
                OnInteractionFailed(playerViewId);
                return;
            }

            OnInteractionStart(playerViewId);

            InstantInteract(player);
        }

        protected virtual void InstantInteract(PlayerController pc) { }

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
                OnInteractionFailed(playerViewId);
                return;
            }


            // 조건 확인
            PlayerController player = playerView.GetComponent<PlayerController>();
            if (!CanInteract(player))
            {
                OnInteractionFailed(playerViewId);
                return;
            }

            OnInteractionStart(playerViewId);

            // BakeProcess 생성
            currentProcess = CreateProcess();
            currentProcess.StartProcess(playerViewId, this);
        }

        [PunRPC]
        protected virtual void RPC_SyncState(int newState)
        {
            state = (InteractionState)newState;
        }
        protected abstract bool CanInteract(PlayerController player);
        protected virtual InteractionProcess CreateProcess() { return null; }
        protected virtual void OnInteractionStart(int playerViewId) // 애니메이션·이펙트 전환용 함수
        {
            // State 동기화 및 UI 동기화
            currentPlayerViewId = playerViewId;
            state = InteractionState.InProgress;
            PhotonView.Find(playerViewId).GetComponent<PlayerController>().interactionState = state;

            photonView.RPC(
                nameof(RPC_SyncState),
                RpcTarget.All,
                (int)state
            );
        }

        protected virtual void OnInteractionSuccess(int playerViewId)
        {
            state = InteractionState.Idle;
            PhotonView.Find(playerViewId).GetComponent<PlayerController>().interactionState = state;
            photonView.RPC(
                nameof(RPC_SyncState),
                RpcTarget.All,
                (int)state
            );

            photonView.RPC(
                nameof(RPC_OnInteractionSuccess),
                RpcTarget.All,
                playerViewId
            );
        }
        [PunRPC]
        protected virtual void RPC_OnInteractionSuccess(int playerViewId)
        {
            //animator.SetTrigger("Success");
            //successEffect.Play();
        }
        protected virtual void OnInteractionFailed(int playerViewId)
        {
            state = InteractionState.Idle;
            PhotonView.Find(playerViewId).GetComponent<PlayerController>().interactionState = state;
            photonView.RPC(
                nameof(RPC_SyncState),
                RpcTarget.All,
                (int)state
            );

            photonView.RPC(
                nameof(RPC_OnInteractionFailed),
                RpcTarget.All,
                playerViewId
            );
        }
        [PunRPC]
        protected virtual void RPC_OnInteractionFailed(int playerViewId)
        {
            //animator.SetTrigger("Failed");
            //failedEffect.Play();
        }
        protected virtual void OnInteractionCanceled(int playerViewId)
        {
            state = InteractionState.Idle;
            PhotonView.Find(playerViewId).GetComponent<PlayerController>().interactionState = state;
            photonView.RPC(
                nameof(RPC_SyncState),
                RpcTarget.All,
                (int)state
            );

            photonView.RPC(
                nameof(RPC_OnInteractionCanceled),
                RpcTarget.All,
                playerViewId
            );
        }
        [PunRPC]
        protected virtual void RPC_OnInteractionCanceled(int playerViewId)
        {
            //animator.SetTrigger("Canceled");
            //canceledEffect.Play();
        }
    }
}