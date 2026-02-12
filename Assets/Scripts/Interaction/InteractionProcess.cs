using Photon.Pun;
using Unity.Burst.CompilerServices;
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
        public abstract void InteractProcess(int playerViewId);

        public virtual void CanceledProcess(int playerViewId)
        {
            owner.OnInteractionCanceled(playerViewId);
            state = InteractionState.Canceled;
            EndProcess(playerViewId);
        }
        public virtual void FailedProcess(int playerViewId)
        {
            owner.OnInteractionFailed(playerViewId);
            state = InteractionState.Failed;
            EndProcess(playerViewId);
        }
        public virtual void SuccessProcess(int playerViewId)
        {
            owner.OnInteractionSuccess(playerViewId);
            state = InteractionState.Success;
            EndProcess(playerViewId);
        }
        protected virtual void EndProcess(int playerViewId)
        {
            if (owner == null) return;
            owner.ClearProcess(); // currentProcess = null
            owner = null;
            currentPlayerViewId = 0;
            state = InteractionState.Idle;
        }
    }
}