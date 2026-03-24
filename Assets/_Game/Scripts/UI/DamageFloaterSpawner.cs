using UnityEngine;
using VS.Core;

namespace VS.UI
{
    public class DamageFloaterSpawner : MonoBehaviour
    {
        public static DamageFloaterSpawner Instance { get; private set; }

        [SerializeField] private DamageFloater floaterPrefab;
        [SerializeField] private float spawnHeightOffset = 0.5f;
        [SerializeField] private int preloadCount = 20;

        private ObjectPool<DamageFloater> _pool;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            if (floaterPrefab != null)
                _pool = new ObjectPool<DamageFloater>(floaterPrefab, preloadCount, transform);
        }

        public void Show(float damage, bool isKill, Vector3 worldPos)
        {
            if (_pool == null) return;
            DamageFloater floater = _pool.Get();
            floater.transform.position = worldPos + Vector3.up * spawnHeightOffset;
            floater.Init(damage, isKill, () => _pool.Return(floater));
        }
    }
}
