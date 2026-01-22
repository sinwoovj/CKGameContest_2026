using UnityEngine;

namespace PhotonTest
{
    [CreateAssetMenu(fileName = "StatusEffectSO",
    menuName = "ScriptableObject/StatusEffect", order = int.MaxValue)]
    public class StatusEffectSO : ScriptableObject
    {
        public StatusEffectType type;
        public float duration;
        public float value;
    }
}
