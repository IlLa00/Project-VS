using System;
using System.Collections.Generic;
using UnityEngine;
using VS.Core;
using VS.Data;
using VS.Player;
using VS.XP;

namespace VS.Enemies
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class EnemyBase : MonoBehaviour
    {
        // 전역 활성 적 리스트 — FindObjectsOfType 완전 대체
        public static readonly List<EnemyBase> ActiveEnemies = new List<EnemyBase>(100);

        [SerializeField] private float contactRadius = 0.5f; // 데미지 판정 반경

        private EnemyData _data;
        private float _currentHp;
        private float _effectiveSpeed;
        private float _effectiveDamage;
        private SpriteRenderer _sr;
        private Transform _player;
        private PlayerStats _playerStats;

        private Action<EnemyBase> _onDeath;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();

            // contactRadius를 콜라이더 크기에 동기화
            var col = GetComponent<CircleCollider2D>();
            col.radius = contactRadius;
        }

        void OnEnable()
        {
            ActiveEnemies.Add(this);
            var pc = PlayerController.Instance;
            if (pc != null)
            {
                _player = pc.Transform;
                _playerStats = pc.GetComponent<PlayerStats>();
            }
        }

        void OnDisable()
        {
            ActiveEnemies.Remove(this);
        }

        public void Init(EnemyData data, Action<EnemyBase> onDeathCallback,
                         float hpMult = 1f, float speedMult = 1f, float damageMult = 1f)
        {
            _data = data;
            _currentHp = data.maxHp * hpMult;
            _effectiveSpeed = data.moveSpeed * speedMult;
            _effectiveDamage = data.contactDamage * damageMult;
            _onDeath = onDeathCallback;

            if (data.sprite != null)
                _sr.sprite = data.sprite;
            _sr.color = data.color;
        }

        void Update()
        {
            if (_data == null) return;
            if (GameManager.Instance?.State != GameState.Playing) return;

            if (_player == null)
            {
                var pc = PlayerController.Instance;
                if (pc == null) return;
                _player = pc.Transform;
                _playerStats = pc.GetComponent<PlayerStats>();
            }

            // 플레이어 방향으로 직선 이동
            Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
            transform.position += (Vector3)(dir * _effectiveSpeed * Time.deltaTime);

            if (dir.x != 0f)
                _sr.flipX = dir.x < 0f;

            // 거리 기반 데미지 (PlayerStats iFrame이 중복 피해 방지)
            if (Vector2.Distance(transform.position, _player.position) < contactRadius)
                _playerStats?.TakeDamage(_effectiveDamage);
        }

        public void TakeDamage(float amount)
        {
            if (_data == null) return;
            _currentHp -= amount;
            if (_currentHp <= 0f)
                Die();
        }

        private void Die()
        {
            XpOrbSpawner.Instance?.Spawn(transform.position, _data.xpDrop);
            _onDeath?.Invoke(this);
        }
    }
}
