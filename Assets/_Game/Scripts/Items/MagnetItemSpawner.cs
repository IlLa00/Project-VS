using UnityEngine;
using VS.Core;

namespace VS.Items
{
    public class MagnetItemSpawner : PickupSpawnerBase<MagnetItem>
    {
        public static MagnetItemSpawner Instance { get; private set; }

        [SerializeField] [Range(0f, 1f)] private float dropChance = 0.1f;

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
        }

        public void TrySpawn(Vector2 position)
        {
            if (Random.value > dropChance)
                return;

            MagnetItem magnet = Pool.Get();
            magnet.transform.position = position;
            magnet.Init(ReturnToPool);
        }
    }
}
