using System.Collections;
using UnityEngine;

namespace ZaoMeng.Services
{
    /// <summary>
    /// 命中停顿：全局时间缩放管理器。单例即可，因为时间只有一个。
    /// </summary>
    public class HitStop : MonoBehaviour
    {
        [SerializeField] private float defaultDuration = 0.05f;
        [SerializeField] private float timeScale = 0.05f;

        private Coroutine current;

        public void Trigger(float duration)
        {
            if (current != null) StopCoroutine(current);
            current = StartCoroutine(PauseCoroutine(duration));
        }

        public void Trigger() => Trigger(defaultDuration);

        private IEnumerator PauseCoroutine(float duration)
        {
            float original = Time.timeScale;

            Time.timeScale = timeScale;   // 极慢，不是完全 0，保留一点动态感
            yield return new WaitForSecondsRealtime(duration);  // 用真实时间等，不受 timeScale 影响
            Time.timeScale = original;    // 恢复

            current = null;
        }
    }
}
