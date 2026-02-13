using Photon.Pun;
using UnityEngine;

namespace Shurub
{
    public abstract class Ingredient : MonoBehaviourPun
    {
        protected Rigidbody2D rb;
        protected Collider2D col;

        public virtual string IngredientName => "Ingredient";
        public virtual bool IsCuttable => false;
        public virtual bool IsBakable => false;
        public virtual IngredientManager.IngredientType ingredientType => IngredientManager.IngredientType.Count;
        public virtual IngredientManager.SetType setType => IngredientManager.SetType.Count;

        public Sprite[] sprites; //IngredientState 순서에 맞춰서 스프라이트 패치

        public enum HoldState
        {
            None,
            World, 
            Held,  
            Thrown 
        }
        public HoldState holdState;
        public enum IngredientState
        {
            unCooked = 0,
            cooked = 1, // cut, baked
            burned = 2,
            unCookable = 3 // set, plate
        }
        public IngredientState state = IngredientState.unCooked;

        private const float STOP_SPEED = 0.1f;
        private const float STOP_TIME = 0.2f;
        private float stopTimer;

        private Vector3 originalScale;

        // Monohaviour Functions
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

            if (holdState != HoldState.Thrown)
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

        // Callback Functions
        protected virtual void OnEnable()
        {
            IngredientManager.Register(this);
        }

        protected virtual void OnDisable()
        {
            IngredientManager.Unregister(this);
        }

        protected virtual void OnDestroy()
        {
            IngredientManager.Destroy(this);
        }

        public virtual void OnStopped()
        {
            holdState = HoldState.World;
            stopTimer = 0f;

            rb.linearVelocity = Vector2.zero;

            col.enabled = true;
        }

        protected virtual void OnChangedIndegrientState(IngredientState newState)
        {
            state = newState;
            // 스프라이트 바꿔주는 코드
            this.GetComponent<SpriteRenderer>().sprite = sprites[(int)state];
        }
        public void ChangeTransform(Vector3 pos)
        {
            transform.position = pos;
            photonView.RPC(
                nameof(RPC_ChangeTransform),
                RpcTarget.All,
                pos
            );
        }
        [PunRPC]
        protected void RPC_ChangeTransform(Vector3 pos)
        {
            transform.position = pos;
        }
        public void ChangeSpriteSortingOrder(int order)
        {
            photonView.RPC(
                nameof(RPC_ChangeSpriteSortingLayer),
                RpcTarget.All,
                order
            );
        }
        [PunRPC]
        protected void RPC_ChangeSpriteSortingLayer(int order)
        {
            this.GetComponent<SpriteRenderer>().sortingOrder = order;
        }
        public virtual void SetActive(bool val)
        {
            photonView.RPC(
                nameof(RPC_SetActive),
                RpcTarget.All,
                val
            );
        }
        [PunRPC]
        protected virtual void RPC_SetActive(bool val)
        {
            gameObject.SetActive(val);
        }
        public virtual void SetScale(float ratio)
        {
            photonView.RPC(
                nameof(RPC_SetScale),
                RpcTarget.All,
                ratio
            );
        }
        [PunRPC]
        protected virtual void RPC_SetScale(float ratio)
        {
            gameObject.transform.localScale = originalScale * ratio;
        }

        // 테이블 위에 고정
        public void SetOnTable(Transform setPoint)
        {
            transform.position = setPoint.position;
            transform.rotation = setPoint.rotation;

            rb.simulated = false;
            col.enabled = false;
            rb.angularVelocity = 0;

            // 플레이어 감지 안되게 레이어 변경
            gameObject.layer = LayerMask.NameToLayer("OnTable");
        }
        public void UnSetOnTable()
        {
            rb.simulated = true;
            col.enabled = true;

            gameObject.layer = LayerMask.NameToLayer("Ingredient");
        }

        public virtual void OnCooked()
        {
            photonView.RPC(
                nameof(RPC_OnCooked),
                RpcTarget.All
            );
        }
        [PunRPC]
        protected virtual void RPC_OnCooked()
        {
            OnChangedIndegrientState(IngredientState.cooked);
        }
        public virtual void OnBurned()
        {
            photonView.RPC(
                nameof(RPC_OnBurned),
                RpcTarget.All
            );
        }
        [PunRPC]
        protected virtual void RPC_OnBurned()
        {
            OnChangedIndegrientState(IngredientState.burned);
        }

        [PunRPC]
        public virtual void RPC_Pick(int holderViewId)
        {
            PhotonView holderPV = PhotonView.Find(holderViewId);

            holdState = HoldState.Held;

            rb.simulated = false;
            col.enabled = false;

            transform.SetParent(holderPV.transform);
            transform.localPosition = Vector3.zero;
        }

        [PunRPC]
        public virtual void RPC_Drop(Vector2 worldPos, bool val)
        {
            holdState = HoldState.World;

            transform.SetParent(null);
            transform.position = worldPos;

            transform.rotation = Quaternion.identity;
            transform.localScale = originalScale;

            if (val)
            {
                rb.simulated = true;
                col.enabled = true;
            }
        }

        [PunRPC]
        public virtual void RPC_Throw(Vector2 dir, float force)
        {
            holdState = HoldState.Thrown;

            transform.SetParent(null);

            rb.simulated = true;
            col.enabled = true;

            rb.linearVelocity = Vector2.zero;
            if (photonView.IsMine) 
                rb.AddForce(dir.normalized * force, ForceMode2D.Impulse);
        }

        public bool IsSet() => setType != IngredientManager.SetType.Count;
        public bool IsCooked() => state == IngredientState.cooked;
    }
}
