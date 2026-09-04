using System.Collections.Generic;
using UnityEngine;

namespace ZaoMeng.Services
{
    /// <summary>
    /// 泛型对象池：频繁创建销毁的物体（怪物、子弹、特效）预热后反复复用，
    /// </summary>
    public class ObjectPool<T> where T : Component
    {
        private readonly T prefab;
        private readonly Transform poolRoot;
        private readonly Stack<T> pool = new Stack<T>();

        public ObjectPool(T prefab, int prewarm = 0)
        {
            this.prefab = prefab;
            poolRoot = new GameObject($"[Pool] {typeof(T).Name}").transform;
            for (int i = 0; i < prewarm; i++)
            {
                T obj = Object.Instantiate(prefab, poolRoot);
                obj.gameObject.SetActive(false);
                pool.Push(obj);
            }
        }

        public T Get()
        {
            T obj = pool.Count > 0 ? pool.Pop() : Object.Instantiate(prefab, poolRoot);
            obj.gameObject.SetActive(true);
            return obj;
        }

        public void Release(T obj)
        {
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(poolRoot, false);
            pool.Push(obj);
        }

        public int CountInactive => pool.Count;
    }
}