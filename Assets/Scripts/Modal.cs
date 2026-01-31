using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Modal : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private bool isLocked = false;

    private void Start()
    {
        RectTransform rt = GetComponent<RectTransform>();
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    public void Open(string title, string body, bool disableNo, UnityAction yesAction, UnityAction noAction)
    {
        gameObject.SetActive(true);
        isLocked = false;
        yesButton.interactable = true;
        noButton.interactable = true;

        canvasGroup.DOKill();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        titleText.text = title;
        bodyText.text = body;

        if (disableNo)
        {
            noButton.interactable = false;
            noButton.gameObject.SetActive(false);
        }
        else
        {
            noButton.gameObject.SetActive(true);
            noButton.interactable = true;
        }

        yesButton.onClick.RemoveAllListeners();
        yesButton.onClick.AddListener(() =>
        {
            if (isLocked)
            {
                return;
            }

            isLocked = true;
            yesButton.interactable = false;
            noButton.interactable = false;

            yesAction?.Invoke();
            Close();
        });

        noButton.onClick.RemoveAllListeners();
        noButton.onClick.AddListener(() =>
        {
            if (isLocked)
            {
                return;
            }

            isLocked = true;
            yesButton.interactable = false;
            noButton.interactable = false;

            noAction?.Invoke();
            Close();
        });

        canvasGroup.DOFade(1f, 0.1f);
    }

    public void Close()
    {
        canvasGroup.DOKill();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        canvasGroup.DOFade(0f, 0.1f).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }
}
