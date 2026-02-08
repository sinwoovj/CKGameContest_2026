using Photon.Pun;
using UnityEngine;

namespace Shurub
{
    public abstract class InteractionProcess : MonoBehaviourPun 
    {
        protected InteractionState state;
        protected Structure owner;
        protected int currentPlayerViewId;
        protected PlayerController pc;

        public virtual void StartProcess(int playerViewId, Structure structure)
        {
            pc = PhotonView.Find(playerViewId).GetComponent<PlayerController>();
            state = InteractionState.InProgress;
            currentPlayerViewId = playerViewId;
            owner = structure; 
        }

        public virtual void UpdateProcess(float deltaTime)
        {
            //만약 플레이어가 진행 도중 이동키를 통해 움직이게 되면 조리가 취소됨
            if (pc.isWalking)
            {
                CanceledProcess();
            }
        }
        public virtual void InteractProcess() { }
        protected virtual void CanceledProcess()
        {
            owner.OnInteractionCanceled();
            state = InteractionState.Cancelled;
            EndProcess();
        }
        protected virtual void FailedProcess()
        {
            owner.OnInteractionFailed();
            state = InteractionState.Failed;
            EndProcess();
        }
        protected virtual void SuccessProcess()
        {
            owner.OnInteractionSuccess();
            state = InteractionState.Success;
            EndProcess();
        }
        protected void EndProcess()
        {
            if (owner == null) return;
            owner.ClearProcess(); // currentProcess = null
            owner = null;
            currentPlayerViewId = 0;
            state = InteractionState.Idle;
        }
    }
}