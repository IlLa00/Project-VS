using UnityEngine;
using VS.Core;
using VS.Data;
using VS.Enemies;
using VS.Player;

namespace VS.Weapons
{
    public class ProjectileWeapon : MonoBehaviour, IUpgradableWeapon
    {
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private WeaponData data;

        private const int MAX_UPGRADE = 5;
        private const float SPREAD_STEP = 15f;

        private ObjectPool<Projectile> _pool;
        private float _fireTimer;
        private PlayerStats _playerStats;
        private int _upgradeLevel;
        private int _projectileCount = 1;

        public int UpgradeLevel => _upgradeLevel;
        public bool CanUpgrade => _upgradeLevel < MAX_UPGRADE;

        public float CooldownProgress
        {
            get
            {
                if (data == null) 
                    return 0f;

                float interval = 1f / (data.fireRate * (_playerStats?.FireRateMultiplier ?? 1f));
                    return Mathf.Clamp01(_fireTimer / interval);
            }
        }

        void Awake()
        {
            _playerStats = GetComponentInParent<PlayerStats>();
        }

        void Start()
        {
            InitPool();
        }

        public void Init(WeaponData weaponData)
        {
            data = weaponData;
            InitPool();
        }

        private void InitPool()
        {
            if (projectilePrefab == null || data == null) 
                return;

            _pool = new ObjectPool<Projectile>(projectilePrefab, 20, transform);
        }

        void Update()
        {
            if (GameManager.Instance?.State != GameState.Playing)
                return;

            if (data == null || _pool == null)
                return;

            float effectiveFireRate = data.fireRate * (_playerStats?.FireRateMultiplier ?? 1f);
            _fireTimer += Time.deltaTime;
            if (_fireTimer >= 1f / effectiveFireRate)
            {
                _fireTimer = 0f;
                Fire();
            }
        }

        public void AddProjectile()
        {
            if (!CanUpgrade) return;
            _upgradeLevel++;
            _projectileCount++;
        }

        private void Fire()
        {
            SoundManager.Instance?.Play(SoundType.Shoot);
            EnemyBase target = GetNearestEnemy();
            Vector2 baseDir = target != null
                ? ((Vector2)target.transform.position - (Vector2)transform.position).normalized
                : Vector2.up;

            float finalDamage = data.damage * (_playerStats?.DamageMultiplier ?? 1f);
            float startAngle = -(_projectileCount - 1) * SPREAD_STEP * 0.5f;

            for (int i = 0; i < _projectileCount; i++)
            {
                float angle = startAngle + i * SPREAD_STEP;
                Vector2 dir = RotateVector(baseDir, angle);

                Projectile proj = _pool.Get();
                proj.transform.SetParent(null);
                proj.transform.position = transform.position;
                proj.transform.localScale = Vector3.one * data.projectileScale;
                proj.Init(dir, data.projectileSpeed, finalDamage, data.projectileRange,
                          data.pierceCount, ReturnProjectile);
            }
        }

        private void ReturnProjectile(Projectile proj)
        {
            proj.transform.SetParent(transform);
            _pool.Return(proj);
        }

        private static Vector2 RotateVector(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            
            return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
        }

        private EnemyBase GetNearestEnemy()
        {
            EnemyBase nearest = null;
            float minSqrDist = float.MaxValue;

            foreach (EnemyBase enemy in EnemyBase.ActiveEnemies)
            {
                float sqrDist = Vector2.SqrMagnitude((Vector2)enemy.transform.position - (Vector2)transform.position);
                if (sqrDist < minSqrDist)
                {
                    minSqrDist = sqrDist;
                    nearest = enemy;
                }
            }
            return nearest;
        }
    }
}
