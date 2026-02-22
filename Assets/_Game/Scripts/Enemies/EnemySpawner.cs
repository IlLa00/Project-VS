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
        [SerializeField] private int maxEnemies = 150;
        [SerializeField] private float spawnDistance = 12f;

        [Header("난이도 스케일링")]
        [SerializeField] private float initialInterval = 1.5f;  // 초기 스폰 간격
        [SerializeField] private float minInterval = 0.25f;     // 최소 스폰 간격
        [SerializeField] private float maxStatMultiplier = 6f;  // 최대 스탯 배율
        [SerializeField] private float rampDuration = 300f;     // 최대 난이도 도달 시간 (초)

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
                float interval = GetCurrentInterval();
                _spawnTimer = interval;

                int spawnCount = GetSpawnCount();
                for (int i = 0; i < spawnCount; i++)
                    SpawnEnemy();
            }
        }

        private void SpawnEnemy()
        {
            if (enemyTypes == null || enemyTypes.Length == 0) return;

            float t = GetDifficultyT();
            float statMult = Mathf.Lerp(1f, maxStatMultiplier, t);

            EnemyBase enemy = _pool.Get();
            EnemyData data = PickEnemyType(t);
            enemy.transform.position = GetSpawnPosition();
            enemy.Init(data, ReturnToPool,
                hpMult: statMult,
                speedMult: Mathf.Lerp(1f, 1.8f, t),   // 속도는 최대 1.8배 (너무 빠르면 불쾌)
                damageMult: statMult);
        }

        // 생존 시간에 따라 강한 적 타입을 우선 선택
        private EnemyData PickEnemyType(float t)
        {
            // 초반엔 약한 적, 시간이 지날수록 강한 적도 섞임
            int maxIndex = Mathf.Clamp(
                Mathf.FloorToInt(t * enemyTypes.Length) + 1,
                1, enemyTypes.Length);

            return enemyTypes[Random.Range(0, maxIndex)];
        }

        // 후반에 한 번에 여러 마리 스폰
        private int GetSpawnCount()
        {
            float time = GameManager.Instance?.SurvivalTime ?? 0f;
            if (time >= 240f) return 3;
            if (time >= 120f) return 2;
            return 1;
        }

        private float GetCurrentInterval()
        {
            return Mathf.Lerp(initialInterval, minInterval, GetDifficultyT());
        }

        // 0(초반) ~ 1(최대 난이도)
        private float GetDifficultyT()
        {
            float time = GameManager.Instance?.SurvivalTime ?? 0f;
            return Mathf.Clamp01(time / rampDuration);
        }

        private void ReturnToPool(EnemyBase enemy)
        {
            _pool.Return(enemy);
        }

        private Vector3 GetSpawnPosition()
        {
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
