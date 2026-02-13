using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;
using Photon.Pun.Demo.PunBasics;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using UnityEngine.EventSystems;

namespace Shurub
{
    public class UIManager : Singleton<UIManager>
    {
        public bool IsInterrupted { get; private set; }

        private bool isControlCool = false;
        private bool isCreating = false;

        private Dictionary<Type, IUIBase> uis = new Dictionary<Type, IUIBase>();
        private Stack<IUIBase> uiStack = new Stack<IUIBase>();

        protected override void OnAwake()
        {
            IsInterrupted = false;
        }

        protected override void OnDestroyed()
        {
            IsInterrupted = true;
        }

        public void ClearAllUis()
        {
            if (IsInterrupted)
            {
                return;
            }

            while (uiStack.Count > 0)
            {
                IUIBase ui = uiStack.Pop();
                if (ui.IsOpenned())
                {
                    ui.Hide();
                }
            }
        }

        private void Update()
        {
            if (IsInterrupted)
            {
                return;
            }

            if (ModalManager.Instance.GetActiveModalCount() >= 1)
            {
                return;
            }

            if (NetworkManager.Instance.CurrentRoomState == GameState.Lobby)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    ReturnPrevUI();
                }
            }
        }

        public void RegisterUI<T>(T ui) where T : IUIBase
        {
            uis[typeof(T)] = ui;
        }

        public void UnRegisterUI<T>() where T : IUIBase
        {
            uis.Remove(typeof(T));
        }

        public T GetUI<T>() where T : UIBase<T>
        {
            return uis[typeof(T)] as T;
        }

        public bool HasUI<T>() where T : UIBase<T>
        {
            return uis.ContainsKey(typeof(T));
        }

        public void ShowUI<T>(bool hidePrev = true, bool removePrev = false, bool force = false) where T : UIBase<T>
        {
            if (IsInterrupted)
            {
                return;
            }

            if (!force)
            {
                if (isControlCool)
                {
                    return;
                }

                CalcControlCool().Forget();
            }

            if (!uis.TryGetValue(typeof(T), out IUIBase nextUI))
            {
                return;
            }

            if (uiStack.Count > 0 && uiStack.Peek() == nextUI)
            {
                return;
            }

            if (removePrev && uiStack.Count > 0)
            {
                IUIBase curUI = uiStack.Pop();
                if (curUI.NeedConfirmWhenHide)
                {
                    curUI.ConfirmHide(() =>
                    {
                        nextUI.Prepare();
                        uiStack.Push(nextUI);
                        nextUI.Show();
                    }, force);
                }
                else
                {
                    curUI.Hide();
                    nextUI.Prepare();
                    uiStack.Push(nextUI);
                    nextUI.Show();
                }

                return;
            }

            if (hidePrev && uiStack.Count > 0)
            {
                IUIBase curUI = uiStack.Peek();
                if (curUI.NeedConfirmWhenHide)
                {
                    curUI.ConfirmHide(() =>
                    {
                        nextUI.Prepare();
                        uiStack.Push(nextUI);
                        nextUI.Show();
                    }, force);
                }
                else
                {
                    curUI.Hide();
                    nextUI.Prepare();
                    uiStack.Push(nextUI);
                    nextUI.Show();
                }
            }
            else
            {
                uiStack.Push(nextUI);
                nextUI.Show();
            }
        }

        public void HideUI<T>(bool force = false) where T : UIBase<T>
        {
            if (IsInterrupted)
            {
                return;
            }

            if (!force)
            {
                if (isControlCool)
                {
                    return;
                }

                CalcControlCool().Forget();
            }

            if (uis.TryGetValue(typeof(T), out IUIBase ui))
            {
                if (uiStack.Count < 1)
                {
                    return;
                }

                IUIBase curUI = uiStack.Pop();
                if (curUI.NeedConfirmWhenHide)
                {
                    curUI.ConfirmHide(null, force);
                }
                else
                {
                    curUI.Hide();
                }
            }
        }

        public void ReturnPrevUI(bool force = false)
        {
            if (IsInterrupted)
            {
                return;
            }

            if (!force)
            {
                if (isControlCool)
                {
                    return;
                }

                CalcControlCool().Forget();
            }

            if (uiStack.Count <= 1)
            {
                return;
            }

            IUIBase curUI = uiStack.Peek();

            if (curUI.NeedConfirmWhenHide)
            {
                curUI.ConfirmHide(() =>
                {
                    IUIBase removed = uiStack.Pop();
                    IUIBase prevUI = uiStack.Peek();

                    prevUI.Prepare();
                    prevUI.Show();
                }, force);
            }
            else
            {
                IUIBase removed = uiStack.Pop();
                IUIBase prevUI = uiStack.Peek();

                prevUI.Prepare();
                removed.Hide();
                prevUI.Show();
            }
        }

        /// <summary>
        /// 모든 씬에서 존재해야 하는 UI는 DDO 사용 시 씬이 바뀌면 UIManager와의 연결이 풀립니다. 따라서 null check 후 Instantiate
        /// </summary>
        /// <returns></returns>
        public async UniTask CheckAndMakeUI<T>(string path) where T : UIBase<T>
        {
            if (IsInterrupted)
            {
                return;
            }

            if (isCreating || HasUI<T>())
            {
                return;
            }

            isCreating = true;
            Instantiate(Resources.Load<SettingUI>(path));

            if (FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject obj = new GameObject("EventSystem");
                obj.AddComponent<EventSystem>();
                obj.AddComponent<StandaloneInputModule>();
            }

            await UniTask.WaitUntil(() => HasUI<T>(), cancellationToken: this.GetCancellationTokenOnDestroy());
            isCreating = false;
        }

        private async UniTaskVoid CalcControlCool()
        {
            isControlCool = true;
            await UniTask.Delay(GameConstants.UI.CONTROL_COOL_TIME, cancellationToken: this.GetCancellationTokenOnDestroy());
            isControlCool = false;
        }
    }
}
