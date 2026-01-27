using UnityEngine;
using Photon.Pun;

namespace Shurub
{
    public class PlayerManager : MonoBehaviourPun
    {
        
        void Start()
        {
            CameraWork _cameraWork = this.gameObject.GetComponent<CameraWork>();
            if (_cameraWork != null)
            {
                if(photonView.IsMine)
                {
                    _cameraWork.OnStartFollowing();
                }
            }
            else
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }


        //void LeaveRoom()
        //{
        //    GameManager.Instance.LeaveRoom();
        //}

        // if(!photonView.IsMine) { return; } <-- 해당 코드르를 통해 로컬 플레이어인지 아닌지를 검사 해야함.
    }
}

