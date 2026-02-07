using Photon.Pun;
using UnityEngine;

namespace Shurub
{
    public abstract class InteractionProcess : MonoBehaviourPun 
    {
        protected InteractionState state;
        protected float progress;
        protected int playerViewId;
        protected Structure owner;
        protected int currentPlayerViewId;

        public virtual void StartProcess(int playerViewId, Structure structure)
        {
            currentPlayerViewId = playerViewId;
            owner = structure;
        }
        protected virtual void UpdateProcess() { }
        protected void CompleteSuccess()
        {
            EndProcess();
        }
        protected void EndProcess()
        {
            if (owner == null) return;
            owner.ClearProcess(); // currentProcess = null
            owner = null;
        }
    }
}