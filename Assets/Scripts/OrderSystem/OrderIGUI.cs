using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Shurub
{
    public class OrderIGUI : MonoBehaviour
    {

        [SerializeField] private Image IGSprite;
        public void SetSprite(Sprite sprite)
        {
            IGSprite.sprite = sprite;
        }
    }
}