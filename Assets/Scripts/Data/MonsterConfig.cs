using UnityEngine;

namespace ZaoMeng.Data
{
    [CreateAssetMenu(menuName = "ZaoMeng/Monster Config", fileName = "MonsterConfig")]
    public class MonsterConfig : ScriptableObject
    {
        [Header("属性")]
        [Min(1)] public int MaxHp = 3;
        [Min(0)] public int AttackDamage = 1;

        [Header("AI")]
        [Min(0.1f)] public float PatrolSpeed = 1.5f;
        [Min(0.1f)] public float ChaseSpeed = 3f;
        [Min(0.1f)] public float DetectRange = 5f;
        [Min(0.1f)] public float LoseRange = 7f;
        [Min(0.1f)] public float AttackRange = 1.2f;
        [Min(0.1f)] public float AttackDuration = 0.8f;
        [Min(0f)] public float HurtDuration = 0.3f;
        [Min(0f)] public float KnockbackForce = 4f;
        [Min(0.1f)] public float DeathDuration = 0.8f;
        [Min(0f)] public float KnockbackUp = 2f;   // 受击垂直弹起
    }
}