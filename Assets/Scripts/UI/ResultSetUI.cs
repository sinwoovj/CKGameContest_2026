using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Shurub
{
    public class ResultSetUI : MonoBehaviourPun
    {
        [SerializeField]
        private Image setImage;
        public void SetPos(Vector3 pos)
        {
            transform.position = pos;
        }
        public void SetSprite(int setType)
        {
            setImage.sprite = IngredientManager.Instance.setSprites[setType];
        }
        public void SetActive(bool val)
        {
            gameObject.SetActive(val);
        }
    }
}