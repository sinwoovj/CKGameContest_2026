using Photon.Pun;
using UnityEngine;

namespace Shurub
{   
    public class TestManager : MonoBehaviourPun
    {
        [SerializeField]
        public string TestObjPath;
        [SerializeField]
        public Vector3 TestPos;
        [SerializeField]
        public int spawnCount;

        void Start()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                for (int i = 0; i < spawnCount; i++)
                {
                    InstantiateTest();
                }
            }
        }

        public void InstantiateTest()
        {
            PhotonNetwork.Instantiate("Prefabs/" + TestObjPath, TestPos, Quaternion.identity);
        }
    }
}