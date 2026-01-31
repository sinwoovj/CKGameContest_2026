using Photon.Pun;
using UnityEngine;

namespace Shurub
{   
    public class TestManager : MonoBehaviourPun
    {
        public static TestManager Instance;

        [SerializeField]
        public string TestObjPath;
        [SerializeField]
        public Vector3 TestPos;
        [SerializeField]
        public int spawnCount;

        void Start()
        {
            Instance = this;
        }

        public void InstantiateTest()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                for (int i = 0; i < spawnCount; i++)
                {
                    PhotonNetwork.Instantiate("Prefabs/" + TestObjPath, TestPos, Quaternion.identity);
                }
            }
        }
    }
}