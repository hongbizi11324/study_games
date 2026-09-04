using UnityEngine;
using UnityEngine.UI;
using ZaoMeng.Events;

namespace ZaoMeng.UI
{
    /// <summary>血条（表现层）：只订阅事件、只改自己的 Image——不认识 Player。</summary>
    public class HealthBarUI : MonoBehaviour
    {
        [SerializeField] private PlayerHealthEvent healthEvent;
        [SerializeField] private Image fill;

        private void OnEnable() => healthEvent.Register(OnHealthChanged);
        private void OnDisable() => healthEvent.Unregister(OnHealthChanged);

        private void OnHealthChanged(int current, int max)
        {
            fill.fillAmount = max > 0 ? (float)current / max : 0f;
        }
    }
}