using Photon.Pun;
using UnityEngine;

namespace Shurub
{
    public abstract class Ingredient : MonoBehaviourPun
    {
        protected Rigidbody2D rb;
        protected Collider2D col;

        public enum IngredientState
        {
            World, 
            Held,  
            Thrown 
        }
        public IngredientState state;

        private const float STOP_SPEED = 0.1f;
        private const float STOP_TIME = 0.2f;
        private float stopTimer;

        private Vector3 originalScale;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            originalScale = transform.localScale;
        }

        protected virtual void FixedUpdate()
        {
            if (!photonView.IsMine)
                return;

            if (state != IngredientState.Thrown)
                return;

            if (rb.linearVelocity.magnitude < STOP_SPEED)
            {
                stopTimer += Time.fixedDeltaTime;

                if (stopTimer >= STOP_TIME)
                {
                    OnStopped();
                }
            }
            else
            {
                stopTimer = 0f;
            }
        }
        protected virtual void OnEnable()
        {
            IngredientManager.Register(this);
        }

        protected virtual void OnDisable()
        {
            IngredientManager.Unregister(this);
        }

        public virtual void OnStopped()
        {
            state = IngredientState.World;
            stopTimer = 0f;

            rb.linearVelocity = Vector2.zero;

            col.enabled = true;
        }

        [PunRPC]
        public virtual void RPC_Pick(int holderViewId)
        {
            PhotonView holderPV = PhotonView.Find(holderViewId);

            state = IngredientState.Held;

            rb.simulated = false;
            col.enabled = false;

            transform.SetParent(holderPV.transform);
            transform.localPosition = Vector3.zero;
        }

        [PunRPC]
        public virtual void RPC_Drop(Vector2 worldPos)
        {
            state = IngredientState.World;

            transform.SetParent(null);
            transform.position = worldPos;

            transform.rotation = Quaternion.identity;
            transform.localScale = originalScale;

            rb.simulated = true;
            col.enabled = true;
        }

        [PunRPC]
        public virtual void RPC_Throw(Vector2 dir, float force)
        {
            state = IngredientState.Thrown;

            transform.SetParent(null);

            rb.simulated = true;
            col.enabled = true;

            rb.linearVelocity = Vector2.zero;
            if (photonView.IsMine) 
                rb.AddForce(dir.normalized * force, ForceMode2D.Impulse);
        }
    }
}
