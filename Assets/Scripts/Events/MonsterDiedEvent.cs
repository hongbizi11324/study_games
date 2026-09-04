using System;
using UnityEngine;

namespace ZaoMeng.Events
{
    /// <summary>
    /// 怪物死亡事件通道（无载荷）。
    /// Monster 死亡时广播，Spawner 订阅后延迟刷怪。
    /// 用 SO 事件通道而不是直接回调：发布者与订阅者解耦，Spawner 换位置/替换实现都不用改 Monster。
    /// </summary>
    [CreateAssetMenu(menuName = "ZaoMeng/Monster Died Event", fileName = "MonsterDiedEvent")]
    public class MonsterDiedEvent : ScriptableObject
    {
        private event Action handlers;

        public void Raise() => handlers?.Invoke();
        public void Register(Action handler) => handlers += handler;
        public void Unregister(Action handler) => handlers -= handler;
    }
}
