using UnityEngine;

namespace Shurub
{
    public class Movement2D : MonoBehaviour
    {
        private Rigidbody2D rb;

        public Vector2 MoveDirection { get; set; } = Vector3.zero;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            rb.MovePosition(rb.position + (MoveDirection * 
                GetComponent<Shurub.Player>().MoveSpeed * Time.fixedDeltaTime));
        }
    }
}
