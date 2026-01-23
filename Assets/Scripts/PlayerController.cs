using UnityEngine;


namespace PhotonTest
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        private PhotonTest.Player player;

        float h, v;

        KeyCode KeyInteraction = KeyCode.J;
        KeyCode KeyPickUpOrThrow = KeyCode.K;
        bool isPickUp = false;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            
        }

        private void Update()
        {
            KeyInput();
        }

        private void FixedUpdate()
        {
            PlayerMove();
        }

        private void KeyInput()
        {
            h = Input.GetAxisRaw("Horizontal");
            v = Input.GetAxisRaw("Vertical");
            if (Input.GetKeyDown(KeyInteraction)) Interaction();
            if (Input.GetKeyDown(KeyPickUpOrThrow)) ThrowOrPickUp();
        }

        private void PlayerMove()
        {
            Vector2 dir = new Vector2(h, v);

            if (dir.sqrMagnitude > 1f)
                dir.Normalize();

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            Vector2 nextPos = rb.position + dir * player.MoveSpeed * Time.fixedDeltaTime;

            rb.MovePosition(nextPos);
        }

        public void Interaction()
        {
            
        }

        public void ThrowOrPickUp()
        {
            
        }
    }

}
