using System;
using UnityEngine;

namespace ZaoMeng.Events
{
    /// <summary>带载荷的 SO 事件通道：广播玩家血量变化（当前值, 最大值）。</summary>
    [CreateAssetMenu(menuName = "ZaoMeng/Player Health Event", fileName = "PlayerHealthEvent")]
    public class PlayerHealthEvent : ScriptableObject
    {
        private event Action<int, int> handlers;

        public void Raise(int current, int max) => handlers?.Invoke(current, max);
        public void Register(Action<int, int> handler) => handlers += handler;
        public void Unregister(Action<int, int> handler) => handlers -= handler;
    }
}