using Photon.Pun;
using Shurub;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public interface IUIBase
{
    bool NeedConfirmWhenHide { get; }

    void Prepare();
    void Show();
    void Hide();
    void ConfirmHide(UnityAction onCompleted, bool force);
}

public abstract class UIBase<T> : MonoBehaviour, IUIBase where T : UIBase<T>
{
    //public virtual bool CheckDontDestroyOnLoad()
    //{
    //    return false;
    //}

    protected CanvasGroup canvasGroup;

    public virtual bool NeedConfirmWhenHide => false;
    protected virtual string ConfirmTitle => "";
    protected virtual string ConfirmMessage => "";
    protected virtual UnityAction OnConfirmed => null;

    private void Reset()
    {
        AutoBind();
    }

    private void Awake()
    {
        UIManager.Instance.RegisterUI(this as T);

        Prepare();
        AutoBind();
        AutoConnectButtons();
        OnAwake();
    }

    private void OnDestroy()
    {
        UIManager.Instance.UnRegisterUI<T>();
        OnDestroyed();
    }

    protected virtual void OnAwake()
    {

    }

    protected virtual void OnDestroyed()
    {

    }

    private void CanvasSetup()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void Prepare()
    {
        CanvasSetup();
        gameObject.SetActive(true);
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// 직접 호출 금지. UIManager 통해서 호출
    /// </summary>
    public virtual void Show()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    /// <summary>
    /// 직접 호출 금지. UIManager 통해서 호출
    /// </summary>
    public virtual void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// 직접 호출 금지. UIManager 통해서 호출
    /// Hide시 확인이 필요할 때. ex) Room Lobby에서 나가기 시도할 때
    /// </summary>
    public virtual void ConfirmHide(UnityAction onCompleted, bool force)
    {
        if (force)
        {
            OnConfirmed?.Invoke();
            onCompleted?.Invoke();
            Hide();
            return;
        }

        ModalManager.Instance.OpenNewModal(ConfirmTitle, ConfirmMessage, yesAction: () =>
        {
            OnConfirmed?.Invoke();
            onCompleted?.Invoke();
            Hide();
        });
    }

    /// <summary>
    /// [SerializeField]로 선언된 필드에 대해 자동으로 할당합니다.
    /// 필드 이름은 반드시 **첫 글자만 소문자**여야 하며,  
    /// 해당 이름과 일치하는 Hierarchy 내 GameObject는 **첫 글자만 대문자**이고 나머지는 같아야 합니다.
    /// 예: field 이름이 'menuPanel'이면 GameObject 이름은 'MenuPanel'이어야 합니다.
    /// GameObject 또는 Component 타입 필드만 자동으로 할당됩니다.
    /// </summary>
    protected virtual void AutoBind()
    {
        var fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        foreach (var field in fields)
        {
            if (!field.IsDefined(typeof(SerializeField), true)) continue;

            string fieldName = field.Name;
            string objectName = char.ToUpper(fieldName[0]) + fieldName[1..]; // 첫 글자 대문자로 변환

            if (field.FieldType == typeof(GameObject))
            {
                var go = transform.FindDeepChild(objectName);
                if (go != null)
                {
                    field.SetValue(this, go);
                }
            }
            else if (typeof(Component).IsAssignableFrom(field.FieldType))
            {
                var child = transform.FindDeepChild(objectName);
                if (child != null)
                {
                    var comp = child.GetComponent(field.FieldType);
                    if (comp != null)
                    {
                        field.SetValue(this, comp);
                    }
                }
            }
        }
    }

    /// <summary>
    /// [SerializeField]로 선언된 Button 타입 필드에 대해 자동으로 클릭 이벤트를 연결합니다.
    /// 필드 이름은 반드시 "Button"으로 끝나야 하며,  
    /// 연결할 메서드명은 "OnClick + 첫글자가 대문자인 필드 이름"이어야 합니다.
    /// Ex) 필드 이름이 'playButton'이면 연결할 메서드는 'OnClickPlayButton'이어야 합니다.
    /// 기존에 등록된 리스너는 모두 제거됩니다.
    /// </summary>
    protected virtual void AutoConnectButtons()
    {
        var type = GetType();
        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        var methodDict = new Dictionary<string, MethodInfo>();

        foreach (var m in methods)
        {
            if (m.GetParameters().Length == 0 && m.ReturnType == typeof(void))
                methodDict[m.Name] = m;
        }

        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        foreach (var field in fields)
        {
            if (field.FieldType != typeof(Button)) continue;

            var button = field.GetValue(this) as Button;
            if (button == null) continue;

            string fieldName = field.Name;
            if (!fieldName.EndsWith("Button")) continue;

            string methodName = "OnClick" + char.ToUpper(fieldName[0]) + fieldName[1..];

            if (methodDict.TryGetValue(methodName, out var method))
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => method.Invoke(this, null));
            }
        }
    }
}
