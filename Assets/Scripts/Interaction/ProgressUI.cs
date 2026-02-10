using System;
using UnityEngine;
using UnityEngine.UI;

namespace Shurub
{
    public class ProgressUI : MonoBehaviour
    {
        Transform target; // 플레이어

        public void Bind(Transform targetTransform)
        {
            target = targetTransform;
        }

        void LateUpdate()
        {
            if (target == null) return;
            transform.position = target.position + Vector3.up;
        }
        public void SetProgress(float value)
        {
            gameObject.GetComponent<Slider>().value = value;
        }
        public void SetActive(bool value)
        {
            gameObject.SetActive(value);
        }
    }
}