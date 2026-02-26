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

        private ObjectPool<Projectile> _pool;
        private float _fireTimer;
        private PlayerStats _playerStats;

        private float _damageBonus;
        private float _fireRateBonus;
        private int   _pierceBonus;

        private const int MAX_UPGRADE = 5;
        private int _upgradeLevel;

        public int UpgradeLevel => _upgradeLevel;
        public bool CanUpgrade => _upgradeLevel < MAX_UPGRADE;

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
            if (projectilePrefab == null || data == null) return;
            _pool = new ObjectPool<Projectile>(projectilePrefab, 20, transform);
        }

        public float CooldownProgress
        {
            get
            {
                if (data == null) return 0f;
                float interval = 1f / ((data.fireRate + _fireRateBonus) * (_playerStats?.FireRateMultiplier ?? 1f));
                return Mathf.Clamp01(_fireTimer / interval);
            }
        }

        void Update()
        {
            if (GameManager.Instance?.State != GameState.Playing) 
                return;

            if (data == null || _pool == null) 
                return;

            float effectiveFireRate = (data.fireRate + _fireRateBonus) * (_playerStats?.FireRateMultiplier ?? 1f);
            _fireTimer += Time.deltaTime;
            if (_fireTimer >= 1f / effectiveFireRate)
            {
                _fireTimer = 0f;
                Fire();
            }
        }

        public void ApplyUpgrade(WeaponStatType stat, float value)
        {
            if (!CanUpgrade) 
                return;

            _upgradeLevel++;

            switch (stat)
            {
                case WeaponStatType.DamageUp:   _damageBonus    += value;       break;
                case WeaponStatType.FireRateUp: _fireRateBonus  += value;       break;
                case WeaponStatType.PierceUp:   _pierceBonus    += (int)value;  break;
            }
        }

        private void Fire()
        {
            SoundManager.Instance?.Play(SoundType.Shoot);
            EnemyBase target = GetNearestEnemy();
            Vector2 dir = target != null
                ? ((Vector2)target.transform.position - (Vector2)transform.position).normalized
                : Vector2.up;

            Projectile proj = _pool.Get();
            proj.transform.SetParent(null);          
            proj.transform.position = transform.position;
            proj.transform.localScale = Vector3.one * data.projectileScale;

            var sr = proj.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = data.projectileColor;

            float finalDamage = (data.damage + _damageBonus) * (_playerStats?.DamageMultiplier ?? 1f);
            proj.Init(dir, data.projectileSpeed, finalDamage, data.projectileRange,
                      data.pierceCount + _pierceBonus, ReturnProjectile);
        }

        private void ReturnProjectile(Projectile proj)
        {
            proj.transform.SetParent(transform);     
            _pool.Return(proj);
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
