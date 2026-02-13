using System.Collections;
using UnityEngine;

namespace Shurub
{
    [System.Serializable]
    public struct OrderData
    {
        public int orderId;
        public int recipeType; // 음식이름, 재료, 세트타입 등은 pool에서
        //Time
        public float totalTime;
        public float remainingTime;
    }
}