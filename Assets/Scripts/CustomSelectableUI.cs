using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Shurub
{
    public class CustomSelectableUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image targetImage;
        [SerializeField] private Selectable selectable;
        [SerializeField] private bool fillAnimation;

        private float fillStart;
        [SerializeField] private float fillEnd = 1f;
        [SerializeField] private float fillDuration = 0.25f;

        private void Awake()
        {
            if (targetImage == null)
            {
                targetImage = GetComponent<Image>();
            }

            if (selectable == null)
            {
                selectable = GetComponent<Selectable>();
            }
        }

        private void Start()
        {
            if (fillAnimation && targetImage != null)
            {
                fillStart = targetImage.fillAmount;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (fillAnimation && selectable.interactable)
            {
                PlayFillAnimation();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (fillAnimation)
            {
                StopFillAnimation();
            }
        }

        private void PlayFillAnimation()
        {
            if (targetImage == null)
            {
                Debug.LogError("image is null.");
                return;
            }

            targetImage.DOFillAmount(fillEnd, fillDuration).SetEase(Ease.InOutQuad);
        }

        private void StopFillAnimation()
        {
            if (targetImage == null)
            {
                Debug.LogError("image is null.");
                return;
            }

            targetImage.DOFillAmount(fillStart, fillDuration).SetEase(Ease.InOutQuad);
        }
    }
}
