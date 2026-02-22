using UnityEngine;
using VS.Core;

namespace VS.XP
{
    public class XpOrbSpawner : MonoBehaviour
    {
        public static XpOrbSpawner Instance { get; private set; }

        [SerializeField] private XpOrb orbPrefab;
        [SerializeField] private int preloadCount = 50;

        private ObjectPool<XpOrb> _pool;

        void Awake()
        {
            Instance = this;
            _pool = new ObjectPool<XpOrb>(orbPrefab, preloadCount, transform);
        }

        public void Spawn(Vector2 position, float xpAmount)
        {
            XpOrb orb = _pool.Get();
            orb.transform.position = position;
            orb.Init(xpAmount, ReturnOrb);
        }

        private void ReturnOrb(XpOrb orb)
        {
            _pool.Return(orb);
        }
    }
}
