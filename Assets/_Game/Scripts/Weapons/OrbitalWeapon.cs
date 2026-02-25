using System.Collections.Generic;
using UnityEngine;
using VS.Core;
using VS.Data;
using VS.Enemies;

namespace VS.Weapons
{
    public class OrbitalWeapon : MonoBehaviour, IUpgradableWeapon
    {
        [Header("회전 설정")]
        [SerializeField] private float orbitRadius = 2f;
        [SerializeField] private float rotationSpeed = 180f; 

        [Header("피해 설정")]
        [SerializeField] private float damage = 15f;
        [SerializeField] private float hitInterval = 0.5f; 

        [Header("오브 외형")]
        [SerializeField] private float orbSize = 0.25f;
        [SerializeField] private Color orbColor = Color.cyan;
        [SerializeField] private Sprite orbSprite;

        public float Damage => damage;
        public float HitInterval => hitInterval;

        private const int MAX_UPGRADE = 5;
        private const float MAX_ROTATION_SPEED = 900f;
        private int _upgradeLevel;

        public int UpgradeLevel => _upgradeLevel;
        public bool CanUpgrade => _upgradeLevel < MAX_UPGRADE;

        private readonly List<Transform> _orbs = new List<Transform>();
        private float _angle;

        void Awake()
        {
            AddOrb(); 
        }

        void Update()
        {
            if (GameManager.Instance?.State != GameState.Playing) return;

            _angle += rotationSpeed * Time.deltaTime;
            RefreshOrbPositions();
        }

        public void ApplyUpgrade(WeaponStatType stat, float value)
        {
            if (!CanUpgrade) return;
            _upgradeLevel++;
            AddOrb();
            if (_upgradeLevel >= MAX_UPGRADE)
                rotationSpeed = MAX_ROTATION_SPEED;
        }

        public void AddOrb()
        {
            var go = new GameObject($"Orb_{_orbs.Count}");
            go.transform.SetParent(transform);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = orbSprite;
            sr.color = orbColor;
            go.transform.localScale = Vector3.one * (orbSize * 2f);

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f; 

            var hitbox = go.AddComponent<OrbHitbox>();
            hitbox.Init(this);

            _orbs.Add(go.transform);
            RefreshOrbPositions();
        }

        private void RefreshOrbPositions()
        {
            if (_orbs.Count == 0) 
                return;

            float step = 360f / _orbs.Count;
            for (int i = 0; i < _orbs.Count; i++)
            {
                float rad = (_angle + step * i) * Mathf.Deg2Rad;
                _orbs[i].localPosition = new Vector3(
                    Mathf.Cos(rad) * orbitRadius,
                    Mathf.Sin(rad) * orbitRadius,
                    0f);
            }
        }

        public void OnOrbHit(EnemyBase enemy)
        {
            enemy.TakeDamage(damage);
        }
    }

    class OrbHitbox : MonoBehaviour
    {
        private OrbitalWeapon _weapon;
        private readonly Dictionary<EnemyBase, float> _nextHitTime = new Dictionary<EnemyBase, float>();

        public void Init(OrbitalWeapon weapon) => _weapon = weapon;

        void OnTriggerStay2D(Collider2D other)
        {
            if (_weapon == null) return;

            var enemy = other.GetComponent<EnemyBase>();
            if (enemy == null) return;

            if (!_nextHitTime.TryGetValue(enemy, out float t) || Time.time >= t)
            {
                _weapon.OnOrbHit(enemy);
                _nextHitTime[enemy] = Time.time + _weapon.HitInterval;
            }
        }

        void OnDisable()
        {
            _nextHitTime.Clear();
        }
    }
}
