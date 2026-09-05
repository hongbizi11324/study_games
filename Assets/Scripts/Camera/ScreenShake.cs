using System.Collections;
using UnityEngine;
using ZaoMeng.Events;   // ← 关键：ScreenShakeEvent 在 ZaoMeng.Events 命名空间，缺这行类型解析不到

namespace ZaoMeng.Gameplay
{
    /// <summary>
    /// 屏幕震动：挂在主摄像机上。命中/受击时由 ScreenShakeEvent.Request() 触发。
    /// 协程逐帧偏移 transform.localPosition，不扰动父物体。
    /// </summary>
    public class ScreenShake : MonoBehaviour
    {
        [SerializeField] private float defaultDuration = 0.15f;
        [SerializeField] private float defaultMagnitude = 0.1f;
        [SerializeField] private ScreenShakeEvent shakeEvent;   // 拖 ScreenShakeEvent.asset

        private Vector3 originalPosition;
        private Coroutine currentShake;

        private void OnEnable()
        {
            originalPosition = transform.localPosition;
            // SO 事件是全局资产，必须成对注册/注销：
            // 只注册不注销，热重载后同一份 asset 上会叠多个订阅，一次命中震好几次
            shakeEvent?.Register(OnShakeRequested);
        }

        private void OnDisable()
        {
            shakeEvent?.Unregister(OnShakeRequested);
        }

        /// <summary>触发一次屏幕震动，参数：时长（秒）、振幅（世界单位）。</summary>
        public void Shake(float duration, float magnitude)
        {
            if (currentShake != null)
                StopCoroutine(currentShake);

            currentShake = StartCoroutine(ShakeCoroutine(duration, magnitude));
        }

        /// <summary>用默认参数的快捷版。</summary>
        public void Shake() => Shake(defaultDuration, defaultMagnitude);

        private void OnShakeRequested(float duration, float magnitude) => Shake(duration, magnitude);

        private IEnumerator ShakeCoroutine(float duration, float magnitude)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                Vector2 offset = Random.insideUnitCircle * magnitude;
                transform.localPosition = originalPosition + new Vector3(offset.x, offset.y, 0f);

                elapsed += Time.deltaTime;
                yield return null;
            }

            // 结束必须归位，否则相机永久偏移
            transform.localPosition = originalPosition;
            currentShake = null;
        }
    }
}
