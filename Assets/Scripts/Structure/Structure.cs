using System.Collections;
using UnityEngine;

namespace Shurub
{
    public abstract class Structure : MonoBehaviour
    {
        protected Rigidbody2D rb;
        protected Collider2D col;
        protected virtual string StructureName => "Structure";
        protected virtual bool IsInteractable => false;

        // Monohaviour Functions
        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
        }

        protected virtual void Start()
        {

        }

        protected virtual void Update()
        {

        }
    }
}