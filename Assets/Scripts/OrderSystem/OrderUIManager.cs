using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shurub
{
    public class OrderUIManager : MonoBehaviour
    {
        public static OrderUIManager Instance;

        [SerializeField] private GameObject orderUIPrefab;
        [SerializeField] private Transform orderRoot;

        private Dictionary<int, OrderUI> orderMap = new();

        private void Awake()
        {
            Instance = this;
        }

        public void AddOrder(int orderId, int recipeType, float totalTime)
        {
            if (orderMap.ContainsKey(orderId)) return;

            var ui = Instantiate(orderUIPrefab, orderRoot)
                .GetComponent<OrderUI>();

            ui.Init(orderId, recipeType, totalTime);

            orderMap.Add(orderId, ui);
        }

        public void RemoveOrder(int orderId)
        {
            if (!orderMap.TryGetValue(orderId, out var ui)) return;

            Destroy(ui.gameObject);
            orderMap.Remove(orderId);
        }

        public void ClearOrder()
        {
            if (orderMap == null) return;

            foreach (var ui in orderMap.Values)
            {
                Destroy(ui.gameObject);
            }

            orderMap.Clear();
        }
        public void UpdateOrderTime(int orderId, float time)
        {
            if (!orderMap.TryGetValue(orderId, out var ui)) return;
            ui.UpdateTime(time);
        }
    }
}