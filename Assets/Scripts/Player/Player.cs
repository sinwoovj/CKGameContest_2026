using System;
using UnityEngine;

namespace Shurub
{
    public class Player : MonoBehaviour
    {
        // Core State
        // public uint pId; public int pNum; public string nickname; -> Photon.Realtime.Player.CustomProperties
        public bool isAlive;
        public bool isSpawned = false;

        // Transform & Movement
        public Vector2 Pos => this.gameObject.transform.position;
        public Quaternion Rot => this.gameObject.transform.rotation;
        private float defaultSpeed = 5;
        public float influenceSpeed = 1;
        public float MoveSpeed => defaultSpeed * influenceSpeed;
        public float AngularVelocity => this.GetComponent<Rigidbody2D>().angularVelocity;
        public Vector2 LinearVelocity => this.GetComponent<Rigidbody2D>().linearVelocity;
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

    /*
    Network -> Photon.Realtime.Player.CustomProperties
        private bool isLocalPlayer;
        private uint networkId;
        public int ping;
    */

    // Debug

    }    
}
