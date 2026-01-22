using System;
using UnityEngine;

namespace PhotonTest
{
    public class Player : MonoBehaviour
    {
        // Core State
        private uint id;
        public string nickname;
        public bool isAlive;

    // Transform & Movement
        private Vector2 pos;
        private Vector2 rot;
        private float velocity;
        private float moveSpeed;
        public bool isGrounded;

    // Vitals
        public float maxHp;
        public float currentHp;
        public float regenHp;
        public bool isInvincible;
        public float maxStamina;
        public float currentStamina;
        public float regenStamina;

    // Input & Action State
        public bool isMoving;
        public bool isInteracting;

    // Item & Inventory
        private const int maxInventorySize = 3;
        //Item[] inventoryItems = new Item[maxInventorySize];

    // Progression
    /// <summary>
    /// 
    /// </summary>
        public int level;
        public float currentExp;
        public float requiredExp;

    // Status Effects
        StatusEffect[] statusEffects;

    // Camera
        private Player cameraTarget;
        public Vector2 lookDirection;

    // Network
        private bool isLocalPlayer;
        private uint networkId;
        public int ping;

    // Debug

    }
}
