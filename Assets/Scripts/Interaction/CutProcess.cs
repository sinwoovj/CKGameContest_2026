using Photon.Pun;
using UnityEngine;

namespace Shurub
{
    public class CutProcess : InteractionProcess
    {
        private const float PROCESS_MAX_COUNT = 6f;
        int pressCount = 0;

        public override void StartProcess(int playerViewId, Structure structure)
        {
            //값 초기화
            base.StartProcess(playerViewId, structure);
            pressCount = 0;
            owner.UpdateProgress(0);
        }

        // 입력 이벤트 수신
        public override void InteractProcess()
        {
            owner.UpdateProgress((float)++pressCount / PROCESS_MAX_COUNT);
            // 횟수 카운트 증가 및 검사
            if (pressCount >= PROCESS_MAX_COUNT)
            {
                //상호작용 키를 6번 입력 시 성공
                SuccessProcess();
            }
        }
    }
}