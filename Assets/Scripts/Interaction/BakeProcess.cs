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

            owner.UpdateProgress(0, true);
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

            if (playerView != null && playerView.IsMine && currentTimingUI != null)
            {
                bool success = currentTimingUI.CheckTiming();

                owner.photonView.RPC(
                    "RPC_SendTimingResult",
                    RpcTarget.MasterClient,
                    owner.photonView.ViewID,
                    success
                );
            }
        }
        public void ApplyTimingResult(bool success)
        {
            if (success)
            {
                owner.UpdateProgress((float)++pressCount / PROCESS_MAX_COUNT, true);

                if (pressCount >= PROCESS_MAX_COUNT)
                    SuccessProcess();
            }
            else
            {
                if (++failedCount >= PROCESS_FAIL_MAX_COUNT)
                    FailedProcess();
            }
        }

        protected override void EndProcess()
        {
            currentTimingUI.End();
            base.EndProcess();
        }
    }
}