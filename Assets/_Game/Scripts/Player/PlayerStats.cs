using System;
using UnityEngine;

namespace VS.Player
{
    public class PlayerStats : MonoBehaviour
    {
        [Header("기본 스탯")]
        [SerializeField] private float maxHp = 100f;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float damageMultiplier = 1f;
        [SerializeField] private float fireRateMultiplier = 1f;

        [Header("무적 시간")]
        [SerializeField] private float iFrameDuration = 0.5f;

        public float MaxHp => maxHp;
        public float CurrentHp { get; private set; }
        public float MoveSpeed => moveSpeed;
        public float DamageMultiplier => damageMultiplier;
        public float FireRateMultiplier => fireRateMultiplier;
        public bool IsAlive => CurrentHp > 0f;

        // 피격 무적 타이머
        private float _iFrameTimer;
        public bool IsInvincible => _iFrameTimer > 0f;

        public event Action OnDeath;
        public event Action<float, float> OnHpChanged; // current, max

        void Awake()
        {
            CurrentHp = maxHp;
        }

        void Update()
        {
            if (_iFrameTimer > 0f)
                _iFrameTimer -= Time.deltaTime;
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive || IsInvincible) return;

            CurrentHp = Mathf.Max(0f, CurrentHp - amount);
            _iFrameTimer = iFrameDuration;
            OnHpChanged?.Invoke(CurrentHp, maxHp);

            if (CurrentHp <= 0f)
                OnDeath?.Invoke();
        }

        public void Heal(float amount)
        {
            if (!IsAlive) return;
            CurrentHp = Mathf.Min(maxHp, CurrentHp + amount);
            OnHpChanged?.Invoke(CurrentHp, maxHp);
        }

        // 무기 업그레이드로 스탯 증가 시 사용
        public void AddMoveSpeed(float delta) => moveSpeed += delta;
        public void AddDamageMultiplier(float delta) => damageMultiplier += delta;
        public void AddFireRateMultiplier(float delta) => fireRateMultiplier += delta;
        public void AddMaxHp(float delta)
        {
            maxHp += delta;
            CurrentHp = Mathf.Min(CurrentHp + delta, maxHp);
            OnHpChanged?.Invoke(CurrentHp, maxHp);
        }
    }
}
