using UnityEngine;
using UnityEngine.InputSystem;

using Photon.Pun;


namespace Shurub
{
    public class PlayerController : MonoBehaviourPun
    {
        private PlayerInput playerInput;
        private Movement2D movement2D;
        private Animator anim;

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

        private void Update()
        {
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
            }
            moveInput = context.ReadValue<Vector2>();
            anim.SetFloat("InputX", moveInput.x);
            anim.SetFloat("InputY", moveInput.y);
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            PlayerManager.Instance.isIntreact = context.canceled ? false : true;
        }

        public void OnPickUpOrThrow(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if(!PlayerManager.Instance.isThrow)
                {
                    if (PlayerManager.Instance.isCarry)
                    {
                        PlayerManager.Instance.isThrow = true;
                    }
                    else
                    {
                        if (PlayerManager.Instance.isCarrible)
                        {
                            PlayerManager.Instance.isCarry = true;
                        }
                    }
                }
            }
        }
    }

}
