using UnityEngine;
using VS.Core;
using VS.Data;
using VS.Enemies;
using VS.Player;

namespace VS.Weapons
{
    public class WeaponBase : MonoBehaviour
    {
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private WeaponData data;

        private ObjectPool<Projectile> _pool;
        private float _fireTimer;
        private PlayerStats _playerStats;

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

        void Update()
        {
            if (GameManager.Instance?.State != GameState.Playing) return;
            if (data == null || _pool == null) return;

            _fireTimer += Time.deltaTime;
            if (_fireTimer >= 1f / data.fireRate)
            {
                _fireTimer = 0f;
                Fire();
            }
        }

        private void Fire()
        {
            EnemyBase target = GetNearestEnemy();
            Vector2 dir = target != null
                ? ((Vector2)target.transform.position - (Vector2)transform.position).normalized
                : Vector2.up;

            Projectile proj = _pool.Get();
            proj.transform.position = transform.position;
            proj.transform.localScale = Vector3.one * data.projectileScale;

            var sr = proj.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = data.projectileColor;

            float finalDamage = data.damage * (_playerStats?.DamageMultiplier ?? 1f);
            proj.Init(dir, data.projectileSpeed, finalDamage, data.projectileRange, data.pierceCount, ReturnProjectile);
        }

        private void ReturnProjectile(Projectile proj)
        {
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
