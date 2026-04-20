using UnityEngine;

namespace VS.Core
{
    public abstract class PickupSpawnerBase<T> : MonoBehaviour where T : Component
    {
        [SerializeField] protected T prefab;
        [SerializeField] protected int preloadCount = 10;

        protected ObjectPool<T> Pool;

        protected virtual void Awake()
        {
            Pool = new ObjectPool<T>(prefab, preloadCount, transform);
        }

        protected void ReturnToPool(T item) => Pool.Return(item);
    }
}
