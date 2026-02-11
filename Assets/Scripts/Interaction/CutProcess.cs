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
            owner.UpdateProgress(0, true);
        }

        // 입력 이벤트 수신
        public override void InteractProcess(int playerViewId)
        {
            pressCount++;
            owner.UpdateProgress((float)pressCount / PROCESS_MAX_COUNT, true);

            if (pressCount >= PROCESS_MAX_COUNT)
                SuccessProcess();
        }
    }
}