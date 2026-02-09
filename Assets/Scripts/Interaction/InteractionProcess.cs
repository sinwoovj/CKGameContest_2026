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
        public abstract void InteractProcess();

        public virtual void CanceledProcess()
        {
            owner.OnInteractionCanceled();
            state = InteractionState.Canceled;
            EndProcess();
        }
        public virtual void FailedProcess()
        {
            owner.OnInteractionFailed();
            state = InteractionState.Failed;
            EndProcess();
        }
        public virtual void SuccessProcess()
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