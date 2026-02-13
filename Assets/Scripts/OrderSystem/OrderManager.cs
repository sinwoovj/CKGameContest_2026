using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shurub
{
    public class OrderManager : SingletonPun<OrderManager>
    {
        [SerializeField] private List<Recipe> recipePool;
        [SerializeField] private int maxOrderCount = 3;
        [SerializeField] private float spawnInterval = 5f;
        const float HP_PANELTY = -5f;

        private List<OrderData> currentOrders = new();
        private float spawnTimer;

        private int orderIdCounter = 0;
        private void Update()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            HandleTimer();
            EnsureAtLeastOneOrder();
            HandleSpawn();
        }
        void EnsureAtLeastOneOrder()
        {
            if (currentOrders.Count > 0) return;

            SpawnOrderImmediately();
        }
        void SpawnOrderImmediately()
        {
            Recipe recipe = recipePool[Random.Range(0, recipePool.Count)];
            float totalTime = CalculateOrderTime(recipe);

            OrderData order = new OrderData
            {
                orderId = orderIdCounter++,
                recipeType = (int)recipe.setType,
                totalTime = totalTime,
                remainingTime = totalTime
            };

            currentOrders.Add(order);

            photonView.RPC(
                nameof(RPC_AddOrder),
                RpcTarget.All,
                order.orderId,
                order.recipeType,
                totalTime
            );
        }
        public void Init()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            currentOrders.Clear();
            spawnTimer = 0;
        }
        void HandleSpawn()
        {
            if (currentOrders.Count >= maxOrderCount) return;

            spawnTimer += Time.deltaTime;
            if (spawnTimer < spawnInterval) return;

            spawnTimer = 0f;
            SpawnOrderImmediately();
        }

        float CalculateOrderTime(Recipe recipe)
        {
            float time = 0f;

            foreach (var ingredient in recipe.requiredIngredients)
            {
                bool isProcessed = IngredientManager.Instance.IsProcessed(ingredient);
                time += 30f;
                if (isProcessed)
                    time += 15f;
            }

            return time;
        }
        void HandleTimer()
        {
            for (int i = currentOrders.Count - 1; i >= 0; i--)
            {
                OrderData order = currentOrders[i];
                order.remainingTime -= Time.deltaTime;

                if (order.remainingTime <= 0)
                {
                    ApplyHpPenalty();
                    photonView.RPC(nameof(RPC_RemoveOrder), RpcTarget.All, order.orderId);
                    currentOrders.RemoveAt(i);
                }
                else
                {
                    currentOrders[i] = order;

                    photonView.RPC(
                        nameof(RPC_UpdateOrderTime),
                        RpcTarget.All,
                        order.orderId,
                        order.remainingTime
                    );
                }
            }
        }
        [PunRPC]
        void RPC_UpdateOrderTime(int orderId, float remainingTime)
        {
            OrderUIManager.Instance.UpdateOrderTime(orderId, remainingTime);
        }
        public void ProcessSubmit(int recipeType)
        {
            for (int i = 0; i < currentOrders.Count; i++)
            {
                if (currentOrders[i].recipeType == recipeType)
                {
                    float ratio = currentOrders[i].remainingTime / currentOrders[i].totalTime;
                    ApplyHpReward(ratio);

                    photonView.RPC(nameof(RPC_RemoveOrder), RpcTarget.All, currentOrders[i].orderId);
                    currentOrders.RemoveAt(i);
                    return;
                }
            }
        }
        void ApplyHpReward(float ratio)
        {
            int heal = 0;

            if (ratio >= 0.5f)
                heal = 10;
            else if (ratio >= 0.25f)
                heal = 5;
            else
                heal = 3;

            InGameManager.Instance.ChangeHP(heal);
        }
        void ApplyHpPenalty()
        {
            InGameManager.Instance.ChangeHP(HP_PANELTY);
        }

        public Recipe GetRecipe(int recipeType)
        {
            return recipePool[recipeType];
        }

        [PunRPC]
        void RPC_AddOrder(int orderId, int recipeType, float totalTime)
        {
            OrderUIManager.Instance.AddOrder(orderId, recipeType, totalTime);
        }

        [PunRPC]
        void RPC_RemoveOrder(int orderId)
        {
            OrderUIManager.Instance.RemoveOrder(orderId);
        }
    }
}