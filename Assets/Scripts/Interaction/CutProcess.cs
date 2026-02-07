using Photon.Pun;
using UnityEngine;

namespace Shurub
{
    public class CutProcess : InteractionProcess
    {
        public override void StartProcess(int playerViewId, Structure structure)
        {
            base.StartProcess(playerViewId, structure);
            // 입력 이벤트 수신
            // 타이밍 판정
            // progress 증가

            //상호작용키를 6번 입력
            //써는 프로세스에서는 플레이어의 애니메이션이나 프로세스의 진척도,
            //그리고 인터렉션을 받아와서 프로세스 로직을 가동
            //만약 플레이어가 진행 도중 이동키를 통해 움직이게 되면 조리가 취소됨
        }
    }
}