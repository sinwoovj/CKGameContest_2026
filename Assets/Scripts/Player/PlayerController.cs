using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Shurub
{
    public class PlayerController : MonoBehaviourPun
    {
        [HideInInspector]
        public PlayerInput playerInput;
        private Movement2D movement2D;
        private Animator anim;

        //Interact Variable
        public float offsetDistance = 0.5f;
        public float castDistance = 0.1f;

        private PlayerManager playerManager;

        //PickUpOrThrow Variable
        public enum HoldState
        {
            Empty,          // 없음
            Holding,        // 들고 있음 (대기)
        }
        public HoldState holdState = HoldState.Empty;

        public Transform holdPoint;
        public Ingredient heldIngredient;

        private bool isCharging;
        private float chargeTimer;

        private const float throwPower = 30f;
        private const float throwChargeTime = 0.3f;

        [SerializeField]
        private Vector2 moveLastInput = Vector2.zero;
        [SerializeField]
        private Vector2 moveInput = Vector2.zero;
        public Vector2 moveDir = Vector2.zero;
        public bool isWalking = false;

        public IInteractable currentInteractable;

        [SerializeField]
        private GameObject progressUIPrefab;
        public ProgressUI progressUI = null;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();

            if(!photonView.IsMine)
            {
                playerInput.enabled = false;
            }

            playerManager = GetComponent<PlayerManager>();
            movement2D = GetComponent<Movement2D>();
            anim = GetComponent<Animator>();
        }

        private void Update()
        {
            if (!photonView.IsMine && PhotonNetwork.IsConnected)
                return;

            if (isCharging)
            {
                chargeTimer += Time.deltaTime;

                if (chargeTimer >= throwChargeTime)
                {
                    ThrowIngredient();   // RPC
                    StopCharging();
                }
            }
            if (isWalking && currentInteractable != null)
            {
                currentInteractable.RequestCancel(photonView.ViewID);
            }

            movement2D.MoveDirection = moveInput;
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            isWalking = true;
            anim.SetBool("IsWalking", true);
            if (context.canceled)
            {
                isWalking = false;
                anim.SetBool("IsWalking", false);
                anim.SetFloat("LastInputX", moveInput.x);
                anim.SetFloat("LastInputY", moveInput.y);
                moveLastInput = moveInput;
                moveDir = moveInput;
            }
            moveInput = context.ReadValue<Vector2>();
            if (!context.canceled) moveDir = moveInput;
            anim.SetFloat("InputX", moveInput.x);
            anim.SetFloat("InputY", moveInput.y);
            if(holdState == HoldState.Holding)
                UpdateIngredientPosition(moveDir);
        }

        private void UpdateIngredientPosition(Vector2 moveDir)
        {

        }
        public void EnsureProcessUI()
        {
            if (progressUI != null)
                return;

            progressUI = Instantiate(progressUIPrefab, gameObject.transform).GetComponent<ProgressUI>();
        }

        public void InitAnim()
        {
            anim.SetFloat("InputX", 0);
            anim.SetFloat("InputY", 0);
            anim.SetFloat("LastInputX", 0);
            anim.SetFloat("LastInputY", 0);
            anim.SetBool("IsWalking", false);
            anim.SetTrigger("Default");
        }

        public void OnInteract(InputAction.CallbackContext context) // K
        {
            if (!context.performed) return;
            if (!photonView.IsMine) return;

            if (currentInteractable == null)
            {
                currentInteractable = DetectInteractable();
                currentInteractable?.Interact(this);
            }
            else
            {
                currentInteractable.InteractProcess(photonView.ViewID);
            }
        }
        private IInteractable DetectInteractable()
        {
            Vector2 dir = moveDir;
            Vector2 origin = (Vector2)transform.position - new Vector2(0, 0.55f);
            Vector2 offsetOrigin = origin + dir.normalized * offsetDistance;
            LayerMask mask = LayerMask.GetMask("Structure");

            if (TestManager.Instance.isTest)
            {
                Debug.DrawRay(offsetOrigin, dir * castDistance, Color.red, 3f);
            }

            RaycastHit2D hit =
                Physics2D.Raycast(offsetOrigin, dir, castDistance, mask);

            if (hit.collider == null)
                return null;

            return hit.collider.GetComponent<IInteractable>();
        }

        public void OnPickUpOrThrow(InputAction.CallbackContext context) // L
        {
            if (!photonView.IsMine) return;

            if (context.started)
            {
                if (holdState == HoldState.Empty)
                {
                    TryPickIngredient();
                }
                else if (holdState == HoldState.Holding)
                {
                    StartCharging();
                }
            }
            else if (context.canceled)
            {
                if (!isCharging)
                    return;

                if (chargeTimer < throwChargeTime)
                {
                    DropIngredient();
                }

                StopCharging();
            }
        }
        void StartCharging()
        {
            isCharging = true;
            chargeTimer = 0f;
        }

        void StopCharging()
        {
            isCharging = false;
            chargeTimer = 0f;
        }

        public void RemoveIngredient()
        {
            IngredientManager.Instance.DestroyIngredient(heldIngredient);
            heldIngredient = null;
            holdState = HoldState.Empty;
        }

        void TryPickIngredient()
        {
            if (holdState != HoldState.Empty)
                return;

            Collider2D hit = Physics2D.OverlapCircle(
                transform.position, 0.5f, LayerMask.GetMask("Ingredient"));

            if (!hit) return;
            if (!hit.TryGetComponent(out Ingredient ingredient)) return;

            photonView.RPC(
                nameof(RPC_PickIngredient),
                RpcTarget.All,
                ingredient.photonView.ViewID
            );
        }
        [PunRPC]
        void RPC_PickIngredient(int ingredientViewId)
        {
            PhotonView pv = PhotonView.Find(ingredientViewId);
            if (!pv) return;

            Ingredient ingredient = pv.GetComponent<Ingredient>();
            if (!ingredient) return;

            heldIngredient = ingredient;
            holdState = HoldState.Holding;

            // Ingredient 쪽 시각적 처리
            ingredient.photonView.RPC(
                nameof(Ingredient.RPC_Pick),
                RpcTarget.All,
                photonView.ViewID
            );
        }

        void DropIngredient()
        {
            if (holdState != HoldState.Holding || heldIngredient == null)
                return;

            photonView.RPC(
                nameof(RPC_DropIngredient),
                RpcTarget.All,
                (Vector2)transform.position
            );
        }
        [PunRPC]
        void RPC_DropIngredient(Vector2 dropPos)
        {
            if (heldIngredient == null)
                return;

            heldIngredient.photonView.RPC(
                nameof(Ingredient.RPC_Drop),
                RpcTarget.All,
                dropPos
            );

            heldIngredient = null;
            holdState = HoldState.Empty;
        }

        void ThrowIngredient()
        {
            if (holdState != HoldState.Holding || heldIngredient == null)
                return;

            Vector2 dir = moveInput != Vector2.zero ? moveInput : moveLastInput;

            photonView.RPC(
                nameof(RPC_ThrowIngredient),
                RpcTarget.All,
                dir,
                throwPower
            );
        }
        [PunRPC]
        void RPC_ThrowIngredient(Vector2 dir, float power)
        {
            if (heldIngredient == null)
                return;

            heldIngredient.photonView.RPC(
                nameof(Ingredient.RPC_Throw),
                RpcTarget.All,
                dir,
                power
            );

            heldIngredient = null;
            holdState = HoldState.Empty;
        }

        public void WarpTo(Vector3 pos)
        {
            photonView.RPC(nameof(WarpToRPC), RpcTarget.All, pos);
        }

        [PunRPC]
        void WarpToRPC(Vector3 pos)
        {
            transform.position = pos;
        }
    }

}
