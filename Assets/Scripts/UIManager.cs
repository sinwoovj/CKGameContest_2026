using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;
using Photon.Pun.Demo.PunBasics;
using Cysharp.Threading.Tasks;

namespace JJM
{
    public class UIManager : Singleton<UIManager>
    {
        public bool IsInterrupted { get; private set; }

        private bool isControlCool = false;

        private Dictionary<Type, UIBase> uis = new Dictionary<Type, UIBase>();
        Stack<UIBase> uiStack = new Stack<UIBase>();

        private void Update()
        {
            if (ModalManager.Instance().GetActiveModalCount() >= 1)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ReturnPrevUI();
            }
        }

        public void RegisterUI<T>(T ui) where T : UIBase
        {
            uis[typeof(T)] = ui;
        }

        public T GetUI<T>() where T : UIBase
        {
            return uis[typeof(T)] as T;
        }

        public void ShowUI<T>(bool hidePrev = true, bool removePrev = false, bool force = false) where T : UIBase
        {
            if (!force)
            {
                if (isControlCool)
                {
                    return;
                }

                CalcControlCool().Forget();
            }

            if (uis.TryGetValue(typeof(T), out UIBase nextUI))
            {
                if (uiStack.Count > 0 && uiStack.Peek() == nextUI)
                {
                    return;
                }

                if (removePrev && uiStack.Count > 0)
                {
                    uiStack.Pop().Hide(() =>
                    {
                        uiStack.Push(nextUI);
                        nextUI.Show();
                    }, force);

                    return;
                }

                if (hidePrev && uiStack.Count > 0)
                {
                    uiStack.Peek().Hide(() =>
                    {
                        uiStack.Push(nextUI);
                        nextUI.Show();
                    }, force);
                }
                else
                {
                    uiStack.Push(nextUI);
                    nextUI.Show();
                }
            }
        }

        public void HideUI<T>(bool force = false) where T : UIBase
        {
            if (!force)
            {
                if (isControlCool)
                {
                    return;
                }

                CalcControlCool().Forget();
            }

            if (uis.TryGetValue(typeof(T), out var ui))
            {
                if (uiStack.Count < 1)
                {
                    return;
                }

                uiStack.Pop().Hide(null, force);
            }
        }

        public void ReturnPrevUI(bool force = false)
        {
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

            uiStack.Peek().Hide(() =>
            {
                uiStack.Pop();
                uiStack.Peek().Show();
            }, force);
        }

        private async UniTaskVoid CalcControlCool()
        {
            isControlCool = true;
            await UniTask.Delay(GameConstants.UI.CONTROL_COOL_TIME, cancellationToken: this.GetCancellationTokenOnDestroy());
            isControlCool = false;
        }
    }
}
