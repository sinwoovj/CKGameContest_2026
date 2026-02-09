using UnityEngine;
using Photon.Pun;

namespace Shurub
{
    public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        //[Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        //public static GameObject LocalPlayerInstance;

        public bool isCarrible = false;

        public bool isCarry = false;
        public bool isThrow = false;
        public bool isIntreact = false;

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(isCarry);
                stream.SendNext(isThrow);
                stream.SendNext(isIntreact);
            }
            else
            {
                // Network player, receive data
                this.isCarry = (bool)stream.ReceiveNext();
                this.isThrow = (bool)stream.ReceiveNext();
                this.isIntreact = (bool)stream.ReceiveNext();
            }
        }

        //private void Awake()
        //{
        //    // #Important
        //    // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        //    if (photonView.IsMine)
        //    {
        //        PlayerManager.LocalPlayerInstance = this.gameObject;
        //    }
        //    // #Critical
        //    // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        //    DontDestroyOnLoad(this.gameObject);
        //}

        //protected override void OnAwake()
        //{
        //    if (photonView.IsMine)
        //    {
        //        LocalPlayerInstance = gameObject;
        //    }
        //}

        void Start()
        {
            CameraWork _cameraWork = this.gameObject.GetComponent<CameraWork>();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, loadingMode) =>
            {
                this.CalledOnLevelWasLoaded(scene.buildIndex);
            };
        }


        void Update()
        {

        }

        void CalledOnLevelWasLoaded(int level)
        {
            
        }
    }
}

