using UnityEngine;
using UnityEngine.InputSystem;


namespace PhotonTest
{
    public class PlayerController : MonoBehaviour
    {
        private Movement2D movement2D;
        private Animator anim;

        private Vector2 moveInput = Vector2.zero;
        bool isPickUp = false;

        private void Awake()
        {
            movement2D = GetComponent<Movement2D>();
            anim = GetComponent<Animator>();
        }

        private void Update()
        {
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
            
        }

        public void OnPickUpOrThrow(InputAction.CallbackContext context)
        {
            
        }
    }

}
