using System;
using UnityEngine;
using UnityEngine.UI;

namespace Shurub
{
    public class ProgressUI : Singleton<ProgressUI>
    {
        [SerializeField] private Slider progressBar;

        public void SetProgress(float value)
        {
            progressBar.value = value;
        }
        public void SetActive(bool value)
        {
            gameObject.SetActive(value);
        }
    }
}