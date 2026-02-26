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
        private float _maxHp;           // 보스 HP바용
        private float _effectiveSpeed;
        private float _effectiveDamage;
        private SpriteRenderer _sr;
        private Transform _player;
        private PlayerStats _playerStats;

        private float _animTimer;
        private int _animFrame;

        private bool _isDying;
        private float _deathAnimTimer;
        private int _deathAnimFrame;

        private Action<EnemyBase> _onDeath;

        /// <summary>HP가 변할 때마다 발행 (current, max). 보스 HP바가 구독.</summary>
        public event Action<float, float> OnHpChanged;

        /// <summary>사망 직전 발행. 보스 HP바 숨기기 등 UI 처리용.</summary>
        public event Action OnDied;

        /// <summary>어떤 적이든 사망 시 발행. KillCountManager가 구독.</summary>
        public static event Action OnAnyEnemyDied;

        public string EnemyName => _data?.enemyName;
        public EnemyType EnemyType => _data?.enemyType ?? default;

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
            _maxHp = data.maxHp * hpMult;
            _currentHp = _maxHp;
            _effectiveSpeed = data.moveSpeed * speedMult;
            _effectiveDamage = data.contactDamage * damageMult;
            _onDeath = onDeathCallback;

            // 이전 구독자 정리 (풀 재사용 시 누적 방지)
            OnHpChanged = null;
            OnDied = null;

            _animTimer = 0f;
            _animFrame = 0;
            _isDying = false;
            _deathAnimTimer = 0f;
            _deathAnimFrame = 0;

            if (data.walkFrames != null && data.walkFrames.Length > 0)
                _sr.sprite = data.walkFrames[0];
            else if (data.sprite != null)
                _sr.sprite = data.sprite;
            _sr.color = data.color;
        }

        void Update()
        {
            if (_data == null) return;
            if (GameManager.Instance?.State != GameState.Playing) return;

            // 사망 애니메이션 재생 중 — 이동/공격 로직 전부 스킵
            if (_isDying)
            {
                _deathAnimTimer += Time.deltaTime;
                float deathFrameDuration = 1f / _data.deathFrameRate;
                if (_deathAnimTimer >= deathFrameDuration)
                {
                    _deathAnimTimer -= deathFrameDuration;
                    _deathAnimFrame++;
                    if (_deathAnimFrame >= _data.deathFrames.Length)
                    {
                        Die();
                        return;
                    }
                    _sr.sprite = _data.deathFrames[_deathAnimFrame];
                }
                return;
            }

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

            // 걷기 애니메이션 (walkFrames가 2개 이상일 때만 작동)
            if (_data.walkFrames != null && _data.walkFrames.Length > 1)
            {
                _animTimer += Time.deltaTime;
                float frameDuration = 1f / _data.animFrameRate;
                if (_animTimer >= frameDuration)
                {
                    _animTimer -= frameDuration;
                    _animFrame = (_animFrame + 1) % _data.walkFrames.Length;
                    _sr.sprite = _data.walkFrames[_animFrame];
                }
            }

            // 거리 기반 데미지 (PlayerStats iFrame이 중복 피해 방지)
            if (Vector2.Distance(transform.position, _player.position) < contactRadius)
                _playerStats?.TakeDamage(_effectiveDamage);
        }

        public void TakeDamage(float amount)
        {
            if (_data == null || _isDying) return;
            _currentHp = Mathf.Max(0f, _currentHp - amount);
            OnHpChanged?.Invoke(_currentHp, _maxHp);
            SoundManager.Instance?.Play(SoundType.EnemyHit);

            if (_currentHp <= 0f)
            {
                if (_data.deathFrames != null && _data.deathFrames.Length > 0)
                    StartDeathAnim();
                else
                    Die();
            }
        }

        private void StartDeathAnim()
        {
            _isDying = true;
            _deathAnimTimer = 0f;
            _deathAnimFrame = 0;
            _sr.sprite = _data.deathFrames[0];
        }

        private void Die()
        {
            SoundManager.Instance?.Play(SoundType.EnemyDie);

            // 엘리트: 사망 시 플레이어에게 일시적 버프
            if (_data.enemyType == EnemyType.Elite)
            {
                PlayerBuffManager.Instance?.ApplyBuff(
                    _data.buffStatType, _data.buffValue, _data.buffDuration);
            }

            OnDied?.Invoke();
            OnAnyEnemyDied?.Invoke();
            XpOrbSpawner.Instance?.Spawn(transform.position, _data.xpDrop);
            _onDeath?.Invoke(this);
        }
    }
}