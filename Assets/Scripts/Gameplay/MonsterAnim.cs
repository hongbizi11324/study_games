using UnityEngine;

namespace ZaoMeng.Gameplay
{
    /// <summary>怪物动画状态名哈希——集中一处生成，杜绝散落的魔法字符串。</summary>
    public static class MonsterAnim
    {
        public static readonly int Idle = Animator.StringToHash("Wuying_Idle");
        public static readonly int Walk = Animator.StringToHash("Wuying_walk");
        public static readonly int Attack = Animator.StringToHash("Wuying_attack1");
        public static readonly int Hurt = Animator.StringToHash("Wuying_hert");
        public static readonly int Dead = Animator.StringToHash("Wuying_Dead");
    }
}