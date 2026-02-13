using UnityEngine;
using UnityEngine.Video;

namespace Shurub
{
    public class IntroUI : UIBase<IntroUI>
    {
        [SerializeField] private VideoPlayer videoPlayer;

        private bool isFinished = false;

        protected override void OnAwake()
        {
            if (GameManager.Instance.IsInitialized)
            {
                return;
            }

            videoPlayer.prepareCompleted += OnVideoPrepared;
            videoPlayer.loopPointReached += OnVideoFinished;
        }

        public override void Show()
        {
            videoPlayer.Prepare();

            base.Show();
        }

        private void Update()
        {
            if (isFinished)
            {
                return;
            }

            if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
            {
                Skip();
            }
        }

        private void OnVideoPrepared(VideoPlayer source)
        {
            source.Play();
        }

        private void OnVideoFinished(VideoPlayer source)
        {
            isFinished = true;
            UIManager.Instance.ShowUI<TitleUI>(removePrev: true);
        }

        private void Skip()
        {
            isFinished = true;
            videoPlayer.Stop();
            UIManager.Instance.ShowUI<TitleUI>(removePrev: true);
        }
    }
}
