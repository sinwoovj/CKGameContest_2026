using Photon.Pun;
using UnityEngine;

namespace Shurub
{
    public class BakeProcess : InteractionProcess
    {
        public override void StartProcess(int playerViewId, Structure structure)
        {
            base.StartProcess(playerViewId, structure);
            // 입력 이벤트 수신
            // 타이밍 판정
            // progress 증가

            //타이밍 맞게 입력( 데바데 발전기 시스템) (재료 당 3번의 기회를 줌)
            //(만약 모두 실패시, 재료가 타버림)
            //굽는 프로세스에서는 플레이어의 애니메이션이나 프로세스의 진척도,
            //그리고 인터렉션을 받아와서 프로세스 로직을 가동
            //만약 플레이어가 진행 도중 이동키를 통해 움직이게 되면 조리가 취소됨
        }
    }
}