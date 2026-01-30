using UnityEngine;

namespace Shurub
{
    public abstract class Ingredient : MonoBehaviour
    {
        public bool isHeld;
        protected Rigidbody2D rb;
        protected Collider2D col;

        private Vector3 originalScale;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            originalScale = transform.localScale;
        }

        public virtual void Pick(Transform holder)
        {
            isHeld = true;
            rb.simulated = false;
            col.enabled = false;

            transform.SetParent(holder);
            transform.localPosition = Vector3.zero;
        }

        public virtual void Drop()
        {
            isHeld = false;
            transform.SetParent(null);

            transform.rotation = Quaternion.identity;
            transform.localScale = originalScale;

            rb.simulated = true;
            col.enabled = true;
        }

        public virtual void Throw(Vector2 dir, float force)
        {
            Drop();
            rb.AddForce(dir.normalized * force, ForceMode2D.Impulse);
        }
    }
}
