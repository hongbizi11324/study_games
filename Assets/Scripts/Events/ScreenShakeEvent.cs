using System;
using UnityEngine;

namespace ZaoMeng.Events
{
    /// <summary>
    /// 屏幕震动事件通道（载荷：时长、振幅）。
    /// 发布端调 Request(duration, magnitude)；订阅端（ScreenShake）在 OnEnable 注册。
    /// 与 MonsterDiedEvent 同构：private event + Register/Unregister，
    /// 这样外部无法用 = 或 Invoke 直接清空/触发别人的订阅。
    /// </summary>
    [CreateAssetMenu(menuName = "ZaoMeng/Screen Shake Event", fileName = "ScreenShakeEvent")]
    public class ScreenShakeEvent : ScriptableObject
    {
        private event Action<float, float> handlers;

        /// <summary>请求一次震动（发布端调用入口）。</summary>
        public void Request(float duration, float magnitude) => handlers?.Invoke(duration, magnitude);

        public void Register(Action<float, float> handler) => handlers += handler;
        public void Unregister(Action<float, float> handler) => handlers -= handler;
    }
}
