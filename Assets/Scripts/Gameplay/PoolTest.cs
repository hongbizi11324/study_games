using UnityEngine;
using ZaoMeng.Services;

namespace ZaoMeng.Gameplay
{
    public class PoolTest : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer orbPrefab;
        [SerializeField] private bool usePool = true;
        [SerializeField] private float interval = 0.1f;

        private ObjectPool<SpriteRenderer> pool;
        private SpriteRenderer current;
        private float timer;

        private void Awake()
        {
            pool = new ObjectPool<SpriteRenderer>(orbPrefab, prewarm: 8);
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer < interval) return;
            timer -= interval;

            if (current != null)
            {
                if (usePool) pool.Release(current);
                else Destroy(current.gameObject);
            }

            current = usePool ? pool.Get() : Instantiate(orbPrefab);
            current.transform.position = transform.position + new Vector3(Random.Range(-2f, 2f), 0f, 0f);
        }
    }
}