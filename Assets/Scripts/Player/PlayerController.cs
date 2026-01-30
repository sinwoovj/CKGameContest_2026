using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
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

        //PickUpOrThrow Variable
        enum HoldState
        {
            Empty,          // 없음
            Holding,        // 들고 있음 (대기)
            Charging        // 던지기 차징 중
        } 
        HoldState holdState = HoldState.Empty;

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

            if (photonView.IsMine == false && PhotonNetwork.IsConnected == true) return;

            movement2D.MoveDirection = moveInput;
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            anim.SetBool("IsWalking", true);
            if (context.canceled)
            {
                anim.SetBool("IsWalking", false);
                anim.SetFloat("LastInputX", moveInput.x);
                anim.SetFloat("LastInputY", moveInput.y);
                moveLastInput = moveInput;
            }
            moveInput = context.ReadValue<Vector2>();
            anim.SetFloat("InputX", moveInput.x);
            anim.SetFloat("InputY", moveInput.y);
        }

        public void OnInteract(InputAction.CallbackContext context) // J
        {
            PlayerManager.Instance.isIntreact = context.canceled ? false : true;
        }

        public void OnPickUpOrThrow(InputAction.CallbackContext context) // K
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


        void TryPickIngredient()
        {
            if (!photonView.IsMine) return;

            Collider2D hit = Physics2D.OverlapCircle(
                transform.position, 0.5f, LayerMask.GetMask("Ingredient"));

            if (hit && hit.TryGetComponent(out Ingredient ingredient))
            {
                heldIngredient = ingredient;
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
