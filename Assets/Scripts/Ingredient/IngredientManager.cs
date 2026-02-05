using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

namespace Shurub
{
    public class IngredientManager : MonoBehaviourPun
    {
        [HideInInspector]
        public static IngredientManager Instance { get; private set; }
        private static readonly string[] ingredientPrefabPaths = {
            "Prefabs/Ingredient/Lettuce",
            "Prefabs/Ingredient/Apple",
            "Prefabs/Ingredient/Rice",
            "Prefabs/Ingredient/Meat",
            "Prefabs/Ingredient/Fish",
            "Prefabs/Ingredient/Ice",
            "Prefabs/Ingredient/Water",
        };
        public enum IngredientType { Lettuce, Apple, Rice, Meat, Fish, Ice, Water, Count };
        private static readonly List<Ingredient> ingredients = new List<Ingredient>();

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        // 생성
        public void InstantiateIngredient(IngredientType type, Vector3 pos)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            PhotonNetwork.Instantiate(ingredientPrefabPaths[(int)type], pos, Quaternion.identity);
        }

        // 삭제
        public void DestroyIngredient(Ingredient ingredient)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            PhotonNetwork.Destroy(ingredient.gameObject);
        }

        // 등록
        public static void Register(Ingredient ingredient)
        {
            if (!ingredients.Contains(ingredient))
            {
                ingredients.Add(ingredient);
            }
        }

        // 해제
        public static void Unregister(Ingredient ingredient)
        {
            ingredients.Remove(ingredient);
        }

        // 모든 Ingredient 제거
        public void ClearIngredient()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            // 역순 제거 (안전)
            for (int i = ingredients.Count - 1; i >= 0; i--)
            {
                if (ingredients[i] != null)
                {
                    DestroyIngredient(ingredients[i]);
                }
            }

            ingredients.Clear();
        }

        // 외부 조회용
        public IReadOnlyList<Ingredient> Ingredients => ingredients;

        // 특정 타입 검색
        public List<T> GetIngredients<T>() where T : Ingredient
        {
            List<T> result = new List<T>();

            foreach (var ingredient in ingredients)
            {
                if (ingredient is T t)
                    result.Add(t);
            }

            return result;
        }
    }
}
