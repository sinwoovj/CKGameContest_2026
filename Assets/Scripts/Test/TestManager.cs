using Photon.Pun;
using UnityEngine;

namespace Shurub
{   
    public class TestManager : MonoBehaviourPun
    {
        public static TestManager Instance;

        [SerializeField]
        public bool isTest;
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
            if (!isTest) return;
            for (int i = 0; i < spawnCount; i++)
            {
                for (int j = 0; j < (int)IngredientManager.IngredientType.Count; j++)
                {
                    IngredientManager.Instance.InstantiateIngredient((IngredientManager.IngredientType)j, TestPos);
                }
            }
        }
    }
}