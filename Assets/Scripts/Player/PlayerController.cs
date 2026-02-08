using Photon.Pun;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static Shurub.PlayerController;
using static UnityEditor.Progress;


namespace Shurub
{
    public class PlayerController : MonoBehaviourPun
    {
        static public PlayerController Instance;
        [HideInInspector]
        public PlayerInput playerInput;
        private Movement2D movement2D;
        private Animator anim;

        //Interact Variable
        public InteractionState interactionState = InteractionState.Idle;

        public float offsetDistance = 0.5f;
        public float castDistance = 0.1f;

        //PickUpOrThrow Variable
        public enum HoldState
        {
            Empty,          // 없음
            Holding,        // 들고 있음 (대기)
            Charging        // 던지기 차징 중
        }
        public HoldState holdState = HoldState.Empty;

        public Transform holdPoint;
        public Ingredient heldIngredient;

        private float chargeTimer;
        private bool isCharging;

        private const float throwPower = 30f;
        private const float throwChargeTime = 0.3f;

        [SerializeField]
        private Vector2 moveLastInput = Vector2.zero;
        [SerializeField]
        private Vector2 moveInput = Vector2.zero;
        public Vector2 moveDir = Vector2.zero;
        public bool isWalking = false;

        public IInteractable currentInteractable;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();

            if(!photonView.IsMine)
            {
                playerInput.enabled = false;
            }

            movement2D = GetComponent<Movement2D>();
            anim = GetComponent<Animator>();
        }

        private void Start()
        {
            Instance = this;
        }

        private void Update()
        {
            if (photonView.IsMine == false && PhotonNetwork.IsConnected == true) return;

            if (isCharging)
            {
                chargeTimer += Time.deltaTime;
                if (heldIngredient != null)
                {
                    if (chargeTimer >= throwChargeTime)
                    {
                        ThrowIngredient();
                        StopCharging();
                    }
                }
            }
            if (interactionState == InteractionState.InProgress)
            {
                currentInteractable.UpdateProcess(Time.deltaTime);
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
            if (!context.performed)
                return;

            switch (interactionState)
            {
                case InteractionState.Idle:
                {
                    currentInteractable = DetectInteractable();
                    if (currentInteractable == null)
                        return;
                    currentInteractable.Interact(this);
                    break;
                }

                case InteractionState.InProgress:
                    // 진행 중 입력 처리
                    currentInteractable.InteractProcess();
                    break;
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
            if (context.started)
            {
                switch (holdState)
                {
                    case HoldState.Empty:
                        TryPickIngredient();
                        break;

                    case HoldState.Holding:
                        StartCharging();
                        break;
                }
            }
            else if (context.canceled)
            {
                if (holdState != HoldState.Charging)
                    return;

                // 아직 임계 시간 안 넘겼으면 드롭
                if (chargeTimer < throwChargeTime)
                {
                    DropIngredient();
                }

                StopCharging();
            }
        }
        void StartCharging()
        {
            holdState = HoldState.Charging;
            chargeTimer = 0f;
            isCharging = true;
        }
        void StopCharging()
        {
            isCharging = false;
            chargeTimer = 0f;

            if (heldIngredient == null)
                holdState = HoldState.Empty;
            else
                holdState = HoldState.Holding;
        }

        public void RemoveIngredient()
        {
            IngredientManager.Instance.DestroyIngredient(heldIngredient);
            heldIngredient = null;
            holdState = HoldState.Empty;
        }

        void TryPickIngredient()
        {

            Collider2D hit = Physics2D.OverlapCircle(
                transform.position, 0.5f, LayerMask.GetMask("Ingredient"));

            if (hit && hit.TryGetComponent(out Ingredient ingredient))
            {
                heldIngredient = ingredient;

                if (!heldIngredient.photonView.IsMine)
                    heldIngredient.photonView.RequestOwnership();

                ingredient.photonView.RPC(
                    "RPC_Pick",
                    RpcTarget.All,
                    photonView.ViewID
                );
                holdState = HoldState.Holding;
            }
        }

        void DropIngredient()
        {
            if (!heldIngredient.photonView.IsMine)
                heldIngredient.photonView.RequestOwnership();

            heldIngredient.photonView.RPC(
                "RPC_Drop",
                RpcTarget.All,
                (Vector2)transform.position
            );
            heldIngredient = null;
            holdState = HoldState.Empty;
        }

        void ThrowIngredient()
        {
            if (!heldIngredient.photonView.IsMine)
                heldIngredient.photonView.RequestOwnership();

            heldIngredient.photonView.RPC(
                "RPC_Throw",
                RpcTarget.All,
                moveInput != Vector2.zero ? moveInput : moveLastInput, 
                throwPower
            );
            heldIngredient = null;
            holdState = HoldState.Empty;
        }
    }

}
