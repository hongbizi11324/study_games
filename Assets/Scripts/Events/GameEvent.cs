using System;
using UnityEngine;

namespace ZaoMeng.Events
{
    /// <summary>
    /// SO 事件通道：发布者与订阅者通过 Inspector 拖拽的同一份 .asset 会合。
    /// 替代单例 EventBus：无字符串事件名、无全局查找、无悬空订阅。
    /// </summary>
    [CreateAssetMenu(menuName = "ZaoMeng/Game Event", fileName = "NewGameEvent")]
    public class GameEvent : ScriptableObject
    {
        private event Action handlers;

        public void Raise() => handlers?.Invoke();

        public void Register(Action handler) => handlers += handler;
        public void Unregister(Action handler) => handlers -= handler;
    }
}