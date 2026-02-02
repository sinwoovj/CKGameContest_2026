using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;
using Photon.Pun.Demo.PunBasics;
using Cysharp.Threading.Tasks;
using Photon.Pun;

namespace Shurub
{
    public class UIManager : Singleton<UIManager>
    {
        public bool IsInterrupted { get; private set; }
        
        public bool isInGame = false;

        private bool isControlCool = false;


        private Dictionary<Type, UIBase> uis = new Dictionary<Type, UIBase>();
        private Stack<UIBase> uiStack = new Stack<UIBase>();

        public void Clear()
        {
            uiStack.Clear();
        }

        private void Update()
        {
            if (ModalManager.Instance().GetActiveModalCount() >= 1)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape) && !isInGame)
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
                    UIBase curUI = uiStack.Pop();
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
                    UIBase curUI = uiStack.Peek();
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

                UIBase curUI = uiStack.Pop();
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
        public void HideRoomLobbyUI()
        {
            photonView.RPC(
                "RPC_HideRoomLobbyUI",
                RpcTarget.All
            );
        }

        [PunRPC]
        public void RPC_HideRoomLobbyUI()
        {
            this.gameObject.GetComponentInChildren<RoomLobbyUI>().Hide();
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

            UIBase curUI = uiStack.Peek();

            if (curUI.NeedConfirmWhenHide)
            {
                curUI.ConfirmHide(() =>
                {
                    UIBase removed = uiStack.Pop();
                    UIBase prevUI = uiStack.Peek();

                    prevUI.Prepare();
                    prevUI.Show();
                }, force);
            }
            else
            {
                UIBase removed = uiStack.Pop();
                UIBase prevUI = uiStack.Peek();

                prevUI.Prepare();
                removed.Hide();
                prevUI.Show();
            }
        }

        private async UniTaskVoid CalcControlCool()
        {
            isControlCool = true;
            await UniTask.Delay(GameConstants.UI.CONTROL_COOL_TIME, cancellationToken: this.GetCancellationTokenOnDestroy());
            isControlCool = false;
        }
    }
}
