using UnityEngine;
using VS.Core;
using VS.Data;

namespace VS.Enemies
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("프리팹 & 데이터")]
        [SerializeField] private EnemyBase enemyPrefab;
        [SerializeField] private EnemyData[] enemyTypes;

        [Header("스폰 설정")]
        [SerializeField] private int preloadCount = 30;
        [SerializeField] private float spawnInterval = 1.5f;
        [SerializeField] private int maxEnemies = 100;
        [SerializeField] private float spawnDistance = 12f; // 카메라 밖 스폰 거리

        private ObjectPool<EnemyBase> _pool;
        private float _spawnTimer;

        void Start()
        {
            _pool = new ObjectPool<EnemyBase>(enemyPrefab, preloadCount, transform);
        }

        void Update()
        {
            if (GameManager.Instance?.State != GameState.Playing) return;
            if (EnemyBase.ActiveEnemies.Count >= maxEnemies) return;

            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f)
            {
                _spawnTimer = spawnInterval;
                SpawnEnemy();
            }
        }

        private void SpawnEnemy()
        {
            if (enemyTypes == null || enemyTypes.Length == 0) return;

            EnemyBase enemy = _pool.Get();
            EnemyData data = enemyTypes[Random.Range(0, enemyTypes.Length)];
            enemy.transform.position = GetSpawnPosition();
            enemy.Init(data, ReturnToPool);
        }

        private void ReturnToPool(EnemyBase enemy)
        {
            _pool.Return(enemy);
        }

        private Vector3 GetSpawnPosition()
        {
            // 카메라 위치 기준, 화면 밖 랜덤 각도로 스폰
            Vector3 center = Camera.main != null
                ? Camera.main.transform.position
                : Vector3.zero;

            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            return new Vector3(
                center.x + Mathf.Cos(angle) * spawnDistance,
                center.y + Mathf.Sin(angle) * spawnDistance,
                0f);
        }
    }
}
