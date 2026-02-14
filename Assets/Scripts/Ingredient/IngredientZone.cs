using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Shurub
{
    public class IngredientZone : MonoBehaviourPun
    {
        [SerializeField] private IngredientManager.IngredientType ingredientType;
        [SerializeField] private int maxCount = 5;
        [SerializeField] private Vector3 spawnAreaSize;

        private HashSet<int> ingredientViewIds = new();

        private void Update()
        {
            if(NetworkManager.Instance.CurrentRoomState == GameState.Loading)
            {
                Init();
            }
        }

        public void Init()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                ingredientViewIds.Clear();
                SpawnUntilFull();
            }
        }
        void SpawnUntilFull()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            while (ingredientViewIds.Count < maxCount)
            {
                SpawnOne();
            }
        }

        void SpawnOne()
        {
            Vector3 pos = GetRandomPositionInZone();
            GameObject obj = IngredientManager.Instance.InstantiateIngredient(ingredientType, pos);

            int viewId = obj.GetPhotonView().ViewID;
            ingredientViewIds.Add(viewId);

            obj.GetComponent<Ingredient>().SetZone(photonView.ViewID);
        }

        Vector3 GetRandomPositionInZone()
        {
            Vector3 center = transform.position;
            return center + new Vector3(
                Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
                0f,
                Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
            );
        }

        // Ingredient가 Zone 밖으로 나갔다고 보고
        public void NotifyIngredientExit(int ingredientViewId)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            if (!ingredientViewIds.Remove(ingredientViewId)) return;

            SpawnUntilFull();
        }
    }
}