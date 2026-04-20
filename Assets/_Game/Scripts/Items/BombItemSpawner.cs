using UnityEngine;
using VS.Core;

namespace VS.Items
{
    public class BombItemSpawner : PickupSpawnerBase<BombItem>
    {
        [SerializeField] private float spawnInterval = 45f;
        [SerializeField] private float bombDamage = 50f;

        private float _timer;

        protected override void Awake()
        {
            base.Awake();
        }

        void Update()
        {
            if (GameManager.Instance?.State != GameState.Playing)
                return;

            _timer += Time.deltaTime;
            if (_timer >= spawnInterval)
            {
                _timer = 0f;
                SpawnBomb();
            }
        }

        private void SpawnBomb()
        {
            Camera cam = Camera.main;
            Vector3 worldPos = cam.ViewportToWorldPoint(new Vector3(
                Random.Range(0.1f, 0.9f),
                Random.Range(0.1f, 0.9f),
                -cam.transform.position.z));
            worldPos.z = 0f;

            BombItem bomb = Pool.Get();
            bomb.transform.position = worldPos;
            bomb.Init(bombDamage, ReturnToPool);
        }
    }
}
