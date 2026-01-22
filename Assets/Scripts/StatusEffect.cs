using UnityEngine;

namespace PhotonTest
{
    public enum StatusEffectType { Poison = 0, Stun = 1, Slow = 2, Shield = 3 }
    public class StatusEffect : MonoBehaviour
    {
        public StatusEffectType type;
        public float duration;
        public float value;
    }
}
