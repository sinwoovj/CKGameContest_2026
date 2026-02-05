using System.Collections;
using UnityEngine;

namespace Shurub
{
    public class Table : Structure
    {
        protected override string StructureName => "Table";
        protected override bool IsInteractable => true;
        public bool isDish = false; // 접시가 있는지

        // Use this for initialization
        protected override void Start()
        {
            base.Start();

        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();

        }
    }
}