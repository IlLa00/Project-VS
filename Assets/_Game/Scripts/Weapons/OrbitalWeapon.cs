using System.Collections.Generic;
using UnityEngine;
using VS.Core;
using VS.Data;
using VS.Enemies;

namespace VS.Weapons
{
    /// <summary>
    /// 플레이어 주변을 회전하며 닿는 적에게 데미지를 주는 무기.
    /// 레벨업 선택지에서 획득하며, 중복 선택 시 오브 개수가 증가한다.
    /// </summary>
    public class OrbitalWeapon : MonoBehaviour, IStackableWeapon, IUpgradableWeapon
    {
        [Header("회전 설정")]
        [SerializeField] private float orbitRadius = 2f;
        [SerializeField] private float rotationSpeed = 180f; // 도/초

        [Header("피해 설정")]
        [SerializeField] private float damage = 15f;
        [SerializeField] private float hitInterval = 0.5f; // 같은 적 재피격 간격(초)

        [Header("오브 외형")]
        [SerializeField] private float orbSize = 0.25f;
        [SerializeField] private Color orbColor = Color.cyan;
        [SerializeField] private Sprite orbSprite;

        public float Damage => damage;
        public float HitInterval => hitInterval;

        private readonly List<Transform> _orbs = new List<Transform>();
        private float _angle;

        void Awake()
        {
            AddOrb(); // 기본 1개
        }

        void Update()
        {
            if (GameManager.Instance?.State != GameState.Playing) return;

            _angle += rotationSpeed * Time.deltaTime;
            RefreshOrbPositions();
        }

        /// <summary>IStackableWeapon 구현 — 카드 중복 선택 시 오브 1개 추가.</summary>
        public void AddStack() => AddOrb();

        /// <summary>IUpgradableWeapon 구현. 지원하지 않는 stat은 무시.</summary>
        public void ApplyUpgrade(WeaponStatType stat, float value)
        {
            switch (stat)
            {
                case WeaponStatType.DamageUp:        damage        += value; break;
                case WeaponStatType.RotationSpeedUp: rotationSpeed += value; break;
                case WeaponStatType.OrbRadiusUp:     orbitRadius   += value; break;
                // FireRateUp, PierceUp 은 이 무기에 해당 없으므로 무시
            }
        }

        /// <summary>오브를 1개 추가한다.</summary>
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
            col.radius = 0.5f; // localScale이 실제 크기를 결정

            var hitbox = go.AddComponent<OrbHitbox>();
            hitbox.Init(this);

            _orbs.Add(go.transform);
            RefreshOrbPositions();
        }

        private void RefreshOrbPositions()
        {
            if (_orbs.Count == 0) return;
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

    /// <summary>오브 개별 충돌 처리. OrbitalWeapon이 생성하는 내부 컴포넌트.</summary>
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
