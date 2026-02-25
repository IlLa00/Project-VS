using System;
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

        [Header("엘리트 설정")]
        [SerializeField] private EnemyData[] eliteTypes;        // 엘리트 EnemyData 배열
        [SerializeField] [Range(0f, 1f)] private float eliteChance = 0.15f; // 스폰당 엘리트 대체 확률

        [Header("보스 설정")]
        [SerializeField] private EnemyData[] bossTypes;         // 보스 EnemyData 배열
        [SerializeField] private float bossSpawnInterval = 180f;// 보스 등장 간격 (초, 기본 3분)

        /// <summary>보스가 스폰될 때 발행된다. BossHPBarUI 등이 구독.</summary>
        public static event Action<EnemyBase> OnBossSpawned;

        private ObjectPool<EnemyBase> _pool;
        private float _spawnTimer;
        private float _bossTimer;

        void Start()
        {
            _pool = new ObjectPool<EnemyBase>(enemyPrefab, preloadCount, transform);
            _bossTimer = bossSpawnInterval;
        }

        void Update()
        {
            if (GameManager.Instance?.State != GameState.Playing) return;

            // 일반/엘리트 스폰
            if (EnemyBase.ActiveEnemies.Count < maxEnemies)
            {
                _spawnTimer -= Time.deltaTime;
                if (_spawnTimer <= 0f)
                {
                    _spawnTimer = GetCurrentInterval();
                    int spawnCount = GetSpawnCount();
                    for (int i = 0; i < spawnCount; i++)
                        SpawnNormalOrElite();
                }
            }

            // 보스 스폰 타이머
            if (bossTypes != null && bossTypes.Length > 0)
            {
                _bossTimer -= Time.deltaTime;
                if (_bossTimer <= 0f)
                {
                    _bossTimer = bossSpawnInterval;
                    TrySpawnBoss();
                }
            }
        }


        private void SpawnNormalOrElite()
        {
            // 엘리트 배열이 있고, 확률에 걸리면 엘리트 스폰
            if (eliteTypes != null && eliteTypes.Length > 0 && UnityEngine.Random.value < eliteChance)
            {
                SpawnEnemy(eliteTypes[UnityEngine.Random.Range(0, eliteTypes.Length)]);
                return;
            }

            if (enemyTypes == null || enemyTypes.Length == 0) return;
            SpawnEnemy(PickNormalType(GetDifficultyT()));
        }

        private void SpawnEnemy(EnemyData data)
        {
            float t = GetDifficultyT();
            float statMult = Mathf.Lerp(1f, maxStatMultiplier, t);

            EnemyBase enemy = _pool.Get();
            enemy.transform.position = GetSpawnPosition();
            enemy.Init(data, ReturnToPool,
                hpMult: statMult,
                speedMult: Mathf.Lerp(1f, 1.8f, t),
                damageMult: statMult);
        }

        // 생존 시간에 따라 강한 일반 적 타입을 우선 선택
        private EnemyData PickNormalType(float t)
        {
            int maxIndex = Mathf.Clamp(
                Mathf.FloorToInt(t * enemyTypes.Length) + 1,
                1, enemyTypes.Length);
            return enemyTypes[UnityEngine.Random.Range(0, maxIndex)];
        }

        private void TrySpawnBoss()
        {
            foreach (EnemyBase e in EnemyBase.ActiveEnemies)
            {
                if (e.EnemyType == EnemyType.Boss) return;
            }

            EnemyData bossData = bossTypes[UnityEngine.Random.Range(0, bossTypes.Length)];
            EnemyBase boss = _pool.Get();
            boss.transform.position = GetSpawnPosition();

            boss.Init(bossData, ReturnToPool);

            OnBossSpawned?.Invoke(boss);
        }

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

            float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            return new Vector3(
                center.x + Mathf.Cos(angle) * spawnDistance,
                center.y + Mathf.Sin(angle) * spawnDistance,
                0f);
        }
    }
}
