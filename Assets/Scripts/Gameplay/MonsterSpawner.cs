using System.Collections;
using UnityEngine;
using ZaoMeng.Events;
using ZaoMeng.Services;

namespace ZaoMeng.Gameplay
{
    /// <summary>
    /// 怪物生成器：持有对象池，开局刷怪，死一只 3 秒后补一只。
    /// </summary>
    public class MonsterSpawner : MonoBehaviour
    {
        [Header("对象")]
        [SerializeField] private Monster prefab;          // Wuying.prefab
        [SerializeField] private Transform target;        // Player

        [Header("出生点")]
        [SerializeField] private Transform[] spawnPoints; // 摆好的空物体数组

        [Header("事件")]
        [SerializeField] private MonsterDiedEvent diedEvent;

        [Header("参数")]
        [SerializeField] private int initialCount = 2;    // 开局刷几只
        [SerializeField] private float respawnDelay = 3f; // 死后几秒补
        [SerializeField] private int prewarmCount = 3;    // 池里预先造好几只，避免运行时 Instantiate 卡顿

        private ObjectPool<Monster> pool;

        private void Awake()
        {
            // 创建对象池。构造函数会预先 Instantiate prewarmCount 只怪物并隐藏。
            pool = new ObjectPool<Monster>(prefab, prewarmCount);
        }

        private void OnEnable()
        {
            diedEvent.Register(OnMonsterDied);
        }

        private void OnDisable()
        {
            diedEvent.Unregister(OnMonsterDied);
        }

        private void Start()
        {
            // 开局从 spawnPoints 里随机挑 initialCount 个点各刷一只
            for (int i = 0; i < initialCount; i++)
            {
                RespawnOne();
            }
        }

        /// <summary>
        /// 从对象池取一只怪，注入引用，放到随机出生点。
        /// </summary>
        private void RespawnOne()
        {
            if (spawnPoints.Length == 0) return;

            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Monster monster = pool.Get();
            monster.Init(pool, target);
            monster.transform.position = point.position;
        }

        /// <summary>
        /// 订阅 MonsterDiedEvent：任何怪物死亡都会进这里。
        /// 等 3 秒后从随机出生点补一只。
        /// </summary>
        private void OnMonsterDied()
        {
            StartCoroutine(RespawnAfterDelay());
        }

        private IEnumerator RespawnAfterDelay()
        {
            yield return new WaitForSeconds(respawnDelay);
            RespawnOne();
        }
    }
}
