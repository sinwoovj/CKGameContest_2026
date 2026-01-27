using UnityEngine;

namespace Shurub
{
    public class CameraWork : MonoBehaviour
    {
        [Tooltip("The distance in the local x-z plane to the target")]
        [SerializeField]
        private float distance = 7.0f;

        [Tooltip("The height we want the camera to be above the target")]
        [SerializeField]
        private float height = 3.0f;

        [Tooltip("Allow the camera to be offseted vertically from the target, for example giving more view of the sceneray and less ground.")]
        [SerializeField]
        private Vector3 centerOffset = Vector3.zero;

        [Tooltip("Set this as false if a component of a prefab being instanciated by Photon Network, and manually call OnStartFollowing() when and if needed.")]
        [SerializeField]
        private bool followOnStart = false;

        [Tooltip("The Smoothing for the camera to follow the target")]
        [SerializeField]
        private float smoothSpeed = 0.125f;

        // cached transform of the target
        Transform cameraTransform;

        // maintain a flag internally to reconnect if target is lost or camera is switched
        bool isFollowing;

        // Cache for camera offset
        Vector3 cameraOffset = Vector3.zero;

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase
        /// </summary>
        void Start()
        {
            // Start following the target if wanted.
            if (followOnStart)
            {
                OnStartFollowing();
            }
        }


        void LateUpdate()
        {
            // The transform target may not destroy on level load,
            // so we need to cover corner cases where the Main Camera is different everytime we load a new scene, and reconnect when that happens
            if (cameraTransform == null && isFollowing)
            {
                OnStartFollowing();
            }

            // only follow is explicitly declared
            if (isFollowing)
            {
                Follow();
            }
        }

        /// <summary>
        /// Raises the start following event.
        /// Use this when you don't know at the time of editing what to follow, typically instances managed by the photon network.
        /// </summary>
        public void OnStartFollowing()
        {
            cameraTransform = Camera.main.transform;
            isFollowing = true;
            // we don't smooth anything, we go straight to the right camera shot
            Cut();
        }

        /// <summary>
        /// Follow the target smoothly
        /// </summary>
        private void Follow()
        {
            cameraOffset.z = -distance;
            cameraOffset.y = height;

            cameraTransform.position = Vector3.Lerp(cameraTransform.position, this.transform.position + this.transform.TransformVector(cameraOffset), smoothSpeed * Time.deltaTime);

            cameraTransform.LookAt(this.transform.position + centerOffset);
        }


        private void Cut()
        {
            cameraOffset.z = -distance;
            cameraOffset.y = height;

            cameraTransform.position = this.transform.position + this.transform.TransformVector(cameraOffset);

            cameraTransform.LookAt(this.transform.position + centerOffset);
        }
    }
}
/*
플레이어를 따라가는 내부 논리는 간단합니다. 
`Height`를 사용하여 `Distance` 이상에서 뒤질 수 있도록 오프셋이 추가된 원하는 카메라 위치를 계산합니다.
그런 다음 원하는 위치를 따라잡기 위해 움직임을 부드럽게 하기 위해 `Lerping`을 사용하고 
마지막으로 카메라가 항상 플레이어를 가리키도록 간단한 `LookAt`을 사용합니다.

카메라 작업 자체 외에도 중요한 것이 설정되었습니다. 
즉, 언제 행동이 플레이어를 적극적으로 따라야 하는지 제어할 수 있는 능력입니다.
이 점을 이해하는 것이 중요합니다. 
우리는 언제 그 플레이어를 따라갈 수 있는 카메라를 갖고 싶습니까?

일반적으로 플레이어를 항상 따라다닌다면 어떤 일이 벌어질지 상상해 보십시오.
플레이어가 가득 찬 룸에 연결하면 다른 플레이어의 인스턴스에 있는 각각의 `CameraWork` 스크립트가 
"메인 카메라"를 제어하기 위해 싸우게 됩니다.
컴퓨터 뒤에 있는 사용자를 나타내는 로컬 플레이어만 따라가면 됩니다.

카메라가 하나뿐이고 플레이어 인스턴스가 여러 개 있다는 문제를 정의하면 여러 가지 방법을 쉽게 찾을 수 있습니다.

1. `CameraWork` 스크립트를 로컬 플레이어에만 붙이십시오.
2. `CameraWork`의 동작은 따라가야 하는 플레이어가 로컬 플레이어인지 아닌지에 따라 껐다 켜서 조절합니다.
3. 카메라에 `CameraWork`을 붙여 해당 씬에서 로컬 플레이어 인스턴스가 있는지 주의하고 해당 인스턴스만 따릅니다.

이 세 가지 옵션은 완전하지 않고, 더 많은 방법을 찾을 수 있지만, 
이 세 가지 중에서 두 번째 선택지를 임의로 선택할 것입니다.
위의 옵션 중 어느 것도 더 낫거나 더 나쁘지는 않지만, 
이 옵션은 코딩이 가장 적게 필요하고 가장 유연합니다. 
"재미있군..." 이렇게 말씀하실 것 같습니다 :)


- `followOnStart` 필드를 노출시켰으며 네트워크 환경이 아닌 곳에서 사용할 때 true로 설정할 수 있습니다. 
예를 들어, 테스트 신이나 완전히 다른 시나리오에서 말이죠.

- 네트워크 기반 게임에서 실행할 때, 플레이어가 로컬 플레이어라고 감지되면 public 메소드인 
`OnStartFollowing()`를 호출할 것입니다. 
이것은 [플레이어 프리팹 네트워킹](./player-networking#camera_control) 챕터에서 설정되고 
생성했던 `PlayerManager` 스크립트 안에서 수행됩니다.


[이전 파트](./player-prefab).
 */