using UnityEngine;
using VS.Core;
using VS.Enemies;
using VS.Player;

namespace VS.Weapons
{
    public class LightningWeapon : MonoBehaviour, IUpgradableWeapon
    {
        [Header("공격 설정")]
        [SerializeField] private float cooldown = 5f;
        [SerializeField] private float strikeRadius = 4f;   // 플레이어로부터 벼락이 내리칠 수 있는 최대 반경
        [SerializeField] private float damageRadius = 1.5f; // 각 벼락의 피해 범위
        [SerializeField] private float damage = 30f;

        [Header("시각 효과")]
        [SerializeField] private LightningStrike strikePrefab;

        private const int MAX_UPGRADE = 5;
        private const float MAX_UPGRADE_COOLDOWN = 1f;

        private int _upgradeLevel;
        private int _boltCount = 1;
        private float _currentCooldown;
        private float _timer;

        private PlayerStats _playerStats;
        private ObjectPool<LightningStrike> _pool;

        public int UpgradeLevel => _upgradeLevel;
        public bool CanUpgrade => _upgradeLevel < MAX_UPGRADE;

        void Awake()
        {
            _playerStats = GetComponentInParent<PlayerStats>();
        }

        void Start()
        {
            _currentCooldown = cooldown;
            _timer = _currentCooldown;

            if (strikePrefab != null)
                _pool = new ObjectPool<LightningStrike>(strikePrefab, 10, null);
        }

        void Update()
        {
            if (GameManager.Instance?.State != GameState.Playing) 
                return;

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _timer = _currentCooldown;
                for (int i = 0; i < _boltCount; i++)
                    Strike();
            }
        }

        private void Strike()
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = Random.Range(1f, strikeRadius);
            Vector2 strikePos = (Vector2)transform.position
                + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;

            if (Camera.main != null)
            {
                Vector3 min = Camera.main.ViewportToWorldPoint(new Vector3(0.05f, 0.05f, 0f));
                Vector3 max = Camera.main.ViewportToWorldPoint(new Vector3(0.95f, 0.95f, 0f));
                strikePos.x = Mathf.Clamp(strikePos.x, min.x, max.x);
                strikePos.y = Mathf.Clamp(strikePos.y, min.y, max.y);
            }

            float finalDamage = damage * (_playerStats?.DamageMultiplier ?? 1f);
            Collider2D[] hits = Physics2D.OverlapCircleAll(strikePos, damageRadius);
            foreach (var hit in hits)
            {
                EnemyBase enemy = hit.GetComponent<EnemyBase>();
                enemy?.TakeDamage(finalDamage);
            }

            if (_pool != null)
            {
                LightningStrike strike = _pool.Get();
                strike.transform.position = strikePos;
                strike.Play(() => _pool.Return(strike));
            }
        }

        public void Upgrade()
        {
            if (!CanUpgrade)
                return;

            _upgradeLevel++;
            _boltCount++;
            if (_upgradeLevel >= MAX_UPGRADE)
                _currentCooldown = MAX_UPGRADE_COOLDOWN;
        }
    }
}
