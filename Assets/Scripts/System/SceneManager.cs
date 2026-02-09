using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Cysharp.Threading.Tasks;
using TMPro;
using DG.Tweening;
using UnityEngine.Events;

namespace Shurub
{
    public class SceneManager : Singleton<SceneManager>
    {
        public bool IsLoading { get; private set; }

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private Slider progressBar;

        private string loadSceneName;
        private string loadLevelName;

        private UnityAction<Scene, LoadSceneMode> onLoaded = null;

        public bool IsOpenned()
        {
            return canvasGroup.alpha >= 0.999f && canvasGroup.isActiveAndEnabled;
        }

        public void LoadScene(string sceneName, UnityAction<Scene, LoadSceneMode> onLoaded = null)
        {
            this.onLoaded = onLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += LoadSceneEnd;

            loadSceneName = sceneName;
            LoadScene().Forget();
        }

        private async UniTaskVoid LoadScene()
        {
            if (IsLoading)
            {
                return;
            }

            progressBar.value = 0f;
            await canvasGroup.DOFade(1f, 0.4f).OnComplete(() =>
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }).AsyncWaitForCompletion();

            AsyncOperation op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(loadSceneName);
            op.allowSceneActivation = false;

            float progressTimer = 0.0f;
            float loadingTextTimer = 0.0f;
            string loadingText = "Loading";
            int dotCount = 0;
            this.loadingText.text = loadingText;

            while (!op.isDone)
            {
                if (op.progress < 0.9f)
                {
                    progressBar.value = Mathf.Lerp(progressBar.value, op.progress, progressTimer);
                    if (progressBar.value >= op.progress)
                    {
                        progressTimer = 0f;
                    }

                    if (loadingTextTimer >= 0.5f)
                    {
                        loadingTextTimer -= 0.5f;
                        dotCount++;
                        if (dotCount > 3)
                        {
                            dotCount = 0;
                        }

                        loadingText += new string('.', dotCount);
                        this.loadingText.text = loadingText;
                    }
                }
                else
                {
                    progressBar.value = Mathf.Lerp(progressBar.value, 1f, progressTimer);

                    if (progressBar.value == 1.0f)
                    {
                        op.allowSceneActivation = true;
                        break;
                    }
                }

                progressTimer += Time.unscaledDeltaTime;
                loadingTextTimer += Time.unscaledDeltaTime;
                await UniTask.Yield(cancellationToken: this.GetCancellationTokenOnDestroy());
            }
        }

        public void LoadLevel(string sceneName, UnityAction<Scene, LoadSceneMode> onLoaded = null)
        {
            PhotonNetwork.AutomaticallySyncScene = true;

            this.onLoaded = onLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += LoadSceneEnd;

            loadSceneName = sceneName;
            LoadLevel().Forget();
        }

        private async UniTaskVoid LoadLevel()
        {
            if (IsLoading)
            {
                return;
            }

            progressBar.value = 0f;
            await canvasGroup.DOFade(1f, 0.4f).OnComplete(() =>
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }).AsyncWaitForCompletion();

            float progressTimer = 0.0f;
            float loadingTextTimer = 0.0f;
            string loadingText = "Loading";
            int dotCount = 0;
            this.loadingText.text = loadingText;

            if (PhotonNetwork.IsMasterClient)
            {
                AsyncOperation op = PhotonNetwork.LoadLevelAsync(loadSceneName);
                op.allowSceneActivation = false;

                while (!op.isDone)
                {
                    if (op.progress < 0.9f)
                    {
                        progressBar.value = Mathf.Lerp(progressBar.value, op.progress, progressTimer);
                        if (progressBar.value >= op.progress)
                        {
                            progressTimer = 0f;
                        }

                        if (loadingTextTimer >= 0.5f)
                        {
                            loadingTextTimer -= 0.5f;
                            dotCount++;
                            if (dotCount > 3)
                            {
                                dotCount = 0;
                            }

                            loadingText += new string('.', dotCount);
                            this.loadingText.text = loadingText;
                        }
                    }
                    else
                    {
                        progressBar.value = Mathf.Lerp(progressBar.value, 1f, progressTimer);

                        if (progressBar.value == 1.0f)
                        {
                            op.allowSceneActivation = true;
                            break;
                        }
                    }

                    PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
                    {
                        { "loadPg", progressBar.value }
                    });

                    progressTimer += Time.unscaledDeltaTime;
                    loadingTextTimer += Time.unscaledDeltaTime;
                    await UniTask.Yield(cancellationToken: this.GetCancellationTokenOnDestroy());
                }
            }
            else
            {
                while (loadLevelName != UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
                {
                    if (!PhotonNetwork.InRoom)
                    {
                        progressBar.value = Mathf.Lerp(progressBar.value, 1f, progressTimer);
                        break;
                    }

                    progressBar.value = Mathf.Lerp(progressBar.value, (float)PhotonNetwork.CurrentRoom.CustomProperties.Get("loadPg", 0f), progressTimer);

                    if (loadingTextTimer >= 0.5f)
                    {
                        loadingTextTimer -= 0.5f;
                        dotCount++;
                        if (dotCount > 3)
                        {
                            dotCount = 0;
                        }

                        loadingText += new string('.', dotCount);
                        this.loadingText.text = loadingText;
                    }

                    progressTimer += Time.unscaledDeltaTime;
                    loadingTextTimer += Time.unscaledDeltaTime;
                    await UniTask.Yield(cancellationToken: this.GetCancellationTokenOnDestroy());
                }
            }
        }

        void LoadSceneEnd(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.name == loadSceneName)
            {
                LoadSceneEndEvent(scene, loadSceneMode).Forget();
            }

            if (scene.name == loadLevelName)
            {
                PhotonNetwork.AutomaticallySyncScene = false;
                LoadSceneEndEvent(scene, loadSceneMode).Forget();
            }
        }

        async UniTaskVoid LoadSceneEndEvent(Scene scene, LoadSceneMode loadSceneMode)
        {
            await UniTask.WaitUntil(() => loadSceneName == UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, cancellationToken: this.GetCancellationTokenOnDestroy());

            await canvasGroup.DOFade(0f, 0.4f).OnComplete(() =>
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }).AsyncWaitForCompletion();

            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= LoadSceneEnd;

            onLoaded?.Invoke(scene, loadSceneMode);
            onLoaded = null;

            IsLoading = false;
        }
    }
}
