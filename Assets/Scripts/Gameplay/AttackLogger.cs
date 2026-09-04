using UnityEngine;
using ZaoMeng.Events;

namespace ZaoMeng.Gameplay
{
    /// <summary>
    /// 事件订阅者示范：OnEnable 订阅、OnDisable 退订，
    /// 订阅生命周期与组件生命周期严格同步——不存在悬空回调。
    /// </summary>
    public class AttackLogger : MonoBehaviour
    {
        [SerializeField] private GameEvent attackEvent;

        private void OnEnable() => attackEvent.Register(OnAttackRaised);
        private void OnDisable() => attackEvent.Unregister(OnAttackRaised);

        private void OnAttackRaised()
        {
            Debug.Log($"[{name}] 监听到攻击");
        }
    }
}