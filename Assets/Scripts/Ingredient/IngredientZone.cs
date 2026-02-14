using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Shurub
{
    public class IngredientZone : MonoBehaviourPun
    {
        [SerializeField] private IngredientManager.IngredientType ingredientType;
        [SerializeField] private int maxCount = 5;
        [SerializeField] private BoxCollider2D zoneCollider;

        private HashSet<int> ingredientViewIds = new();
        private void Awake()
        {
            ZoneManager.Instance.Register(this);
        }
        private void Start()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Fill();
            }
        }

        void Fill()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            while (ingredientViewIds.Count < maxCount)
            {
                SpawnOne();
            }
        }

        void SpawnOne()
        {
            Vector2 pos = GetRandomPositionInZone();
            GameObject obj = IngredientManager.Instance.InstantiateIngredient(ingredientType, pos);

            int viewId = obj.GetPhotonView().ViewID;
            ingredientViewIds.Add(viewId);

            obj.GetComponent<Ingredient>().SetZone(photonView.ViewID);
        }

        Vector2 GetRandomPositionInZone()
        {
            Bounds b = zoneCollider.bounds;
            return new Vector2(
                Random.Range(b.min.x, b.max.x),
                Random.Range(b.min.y, b.max.y)
            );
        }

        // Ingredient가 Zone 밖으로 나갔다고 보고
        public void NotifyIngredientExit(int ingredientViewId)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            if (!ingredientViewIds.Remove(ingredientViewId)) return;
            Debug.Log("NotifyIngredientExit()");
            Fill();
        }
        public void ResetZone()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            foreach (int viewId in ingredientViewIds)
            {
                PhotonView pv = PhotonView.Find(viewId);
                if (pv != null)
                {
                    IngredientManager.Instance.DestroyIngredient(pv.GetComponent<Ingredient>());
                }
            }

            ingredientViewIds.Clear();
            Fill();
        }
    }
}