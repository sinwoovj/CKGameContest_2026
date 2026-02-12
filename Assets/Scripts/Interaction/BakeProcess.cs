using Photon.Pun;
using UnityEngine;

namespace Shurub
{
    public class BakeProcess : InteractionProcess
    {
        private const float PROCESS_MAX_COUNT = 2f;
        private const float PROCESS_FAIL_MAX_COUNT = 3f;
        private int pressCount = 0;
        private int failedCount = 0;

        [SerializeField]
        private GameObject timingUIPrefab;
        private TimingUI currentTimingUI;

        public override void StartProcess(int playerViewId, Structure structure)
        {
            //값 초기화
            base.StartProcess(playerViewId, structure);
            pressCount = 0;
            failedCount = 0;
            PhotonView playerView = PhotonView.Find(playerViewId);

            if (playerView != null && playerView.IsMine)
            {
                EnsureUI(playerViewId);
            }

            owner.UpdateProgress(playerViewId, 0, true);
        }
        private void EnsureUI(int playerViewId)
        {
            if (currentTimingUI == null)
            {
                currentTimingUI = GameObject.Instantiate(timingUIPrefab, GameObject.Find("UIRootOverlay").transform).GetComponent<TimingUI>();
                currentTimingUI.Init();
            }
        }

        // 입력 이벤트 수신
        public override void InteractProcess(int playerViewId)
        {
            PhotonView playerView = PhotonView.Find(playerViewId);
            if (playerView == null || !playerView.IsMine) return;

            if (currentTimingUI == null) return;

            bool success = currentTimingUI.CheckTiming();

            owner.photonView.RPC(
                nameof(Structure.RPC_SendTimingResult),
                RpcTarget.All,
                playerViewId,
                success
            );
        }
        public void ApplyTimingResult(int playerViewId, bool success)
        {
            PhotonView playerView = PhotonView.Find(playerViewId);
            if (playerView == null || !playerView.IsMine) return;

            if (currentTimingUI == null) return;

            if (success)
            {
                owner.UpdateProgress(playerViewId, (float)++pressCount / PROCESS_MAX_COUNT, true);

                if (pressCount >= PROCESS_MAX_COUNT)
                    SuccessProcess(playerViewId);
                else
                    currentTimingUI.Init();
            }
            else
            {
                if (++failedCount >= PROCESS_FAIL_MAX_COUNT)
                    FailedProcess(playerViewId);
                else
                    currentTimingUI.Init();
            }
        }

        protected override void EndProcess(int playerViewId)
        {
            EndTimingUI(playerViewId);
            base.EndProcess(playerViewId);
        }

        public void EndTimingUI(int playerViewId)
        {
            PhotonView playerView = PhotonView.Find(playerViewId);
            if (playerView == null || !playerView.IsMine) return;

            if (currentTimingUI == null) return;

            currentTimingUI.End();
            currentTimingUI = null;
        }
    }
}