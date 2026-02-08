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

        private const float PROCESS_MAX_TIME = 3f;
        private const float SAFETY_TIME_INTERVAL = 0.5f;
        private const float PROCESS_MAX_GOAL_TIME = 2f;
        private float currTime = 0f;
        private float goalTime = 0f;
        public override void StartProcess(int playerViewId, Structure structure)
        {
            //값 초기화
            base.StartProcess(playerViewId, structure);
            pressCount = 0;
            failedCount = 0;
            owner.UpdateProgress(0);
        }
        private void InitProcess()
        {

        }

        public override void UpdateProcess(float deltaTime)
        {
            base.UpdateProcess(deltaTime);

            //타이밍 맞게 입력( 데바데 발전기 시스템) (재료 당 3번의 기회를 줌)
        }

        // 입력 이벤트 수신
        public override void InteractProcess()
        {
            if(CheckTiming())
            {
                // 횟수 카운트 증가 및 검사
                owner.UpdateProgress((float)++pressCount / PROCESS_MAX_COUNT);
                if (pressCount >= PROCESS_MAX_COUNT)
                {
                    SuccessProcess();
                }
                else
                {
                    InitProcess();
                }
            }
            else
            {
                if(++failedCount >= PROCESS_FAIL_MAX_COUNT)
                {
                    FailedProcess();
                }
                else
                {
                    InitProcess();
                }
            }
        }
        private bool CheckTiming()
        {
            // 타이밍 판정

            return false;
        }
    }
}