using System.Collections;
using UnityEngine;
using VS.Core;
using VS.Data;

namespace VS.Player
{
    /// <summary>
    /// 일시적 스탯 버프를 관리한다. (엘리트 처치 보상 등)
    /// Player GameObject에 PlayerStats와 함께 부착한다.
    /// </summary>
    public class PlayerBuffManager : MonoBehaviour
    {
        public static PlayerBuffManager Instance { get; private set; }

        private PlayerStats _stats;

        void Awake()
        {
            Instance = this;
            _stats = GetComponent<PlayerStats>();
        }

        /// <summary>stat을 value만큼 buffDuration초 동안 증가시킨다.</summary>
        public void ApplyBuff(BuffStatType stat, float value, float duration)
        {
            StartCoroutine(BuffCoroutine(stat, value, duration));
        }

        private IEnumerator BuffCoroutine(BuffStatType stat, float value, float duration)
        {
            ApplyStat(stat, value);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                // LevelUp/GameOver 중엔 타이머 정지 (timeScale = 0)
                if (GameManager.Instance?.State == GameState.Playing)
                    elapsed += Time.deltaTime;
                yield return null;
            }

            ApplyStat(stat, -value);
        }

        private void ApplyStat(BuffStatType stat, float value)
        {
            if (_stats == null) return;
            switch (stat)
            {
                case BuffStatType.DamageUp:   _stats.AddDamageMultiplier(value);   break;
                case BuffStatType.SpeedUp:    _stats.AddMoveSpeed(value);           break;
                case BuffStatType.FireRateUp: _stats.AddFireRateMultiplier(value);  break;
            }
        }
    }
}
