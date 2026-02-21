using System.Collections.Generic;
using UnityEngine;

namespace VS.Core
{
    public class ObjectPool<T> where T : Component
    {
        private readonly Queue<T> _pool = new Queue<T>();
        private readonly T _prefab;
        private readonly Transform _parent;

        public ObjectPool(T prefab, int preloadCount, Transform parent)
        {
            _prefab = prefab;
            _parent = parent;

            for (int i = 0; i < preloadCount; i++)
                Return(CreateNew());
        }

        public T Get()
        {
            T obj = _pool.Count > 0 ? _pool.Dequeue() : CreateNew();
            obj.gameObject.SetActive(true);
            return obj;
        }

        public void Return(T obj)
        {
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }

        private T CreateNew()
        {
            T obj = Object.Instantiate(_prefab, _parent);
            obj.gameObject.SetActive(false);
            return obj;
        }
    }
}
