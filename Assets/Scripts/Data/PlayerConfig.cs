using UnityEngine;

namespace ZaoMeng.Data
{
    /// <summary>
    /// 玩家数值配置（数据层）：所有数值集中在此资产，改数值不动代码。
    /// </summary>
    [CreateAssetMenu(menuName = "ZaoMeng/Player Config", fileName = "PlayerConfig")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("移动")]
        [Min(1)] public int MaxHp = 5;
        [Min(0.1f)] public float WalkSpeed = 3f;
        [Min(0.1f)] public float RunSpeed = 6f;
        [Min(1f)] public float JumpForce = 12f;

        [Header("攻击")]
        [Min(1)] public int AttackDamage = 1;
        [Range(0f, 1f)] public float CritChance = 0.1f;     // 暴击率
        [Min(1f)] public float CritMultiplier = 2f;         // 暴击倍率

        [Header("攻击判定框（选中 Player 时红框可视化）")]
        public Vector2 HitboxSize = new Vector2(0.8f, 0.8f);
        public float HitboxOffsetX = 0.8f;                   // 判定框离角色中心距离
    }
}