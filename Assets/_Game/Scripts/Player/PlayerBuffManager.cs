using System.Collections;
using UnityEngine;
using VS.Core;
using VS.Data;

namespace VS.Player
{
    public class PlayerBuffManager : MonoBehaviour
    {
        public static PlayerBuffManager Instance { get; private set; }

        private PlayerStats _stats;

        void Awake()
        {
            Instance = this;
            _stats = GetComponent<PlayerStats>();
        }

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
                if (GameManager.Instance?.State == GameState.Playing)
                    elapsed += Time.deltaTime;
                yield return null;
            }

            ApplyStat(stat, -value);
        }

        private void ApplyStat(BuffStatType stat, float value)
        {
            if (_stats == null) 
                return;

            switch (stat)
            {
                case BuffStatType.DamageUp:
                    _stats.AddDamageMultiplier(value);   
                    break;
                case BuffStatType.SpeedUp:    
                    _stats.AddMoveSpeed(value);           
                    break;
                case BuffStatType.FireRateUp: 
                    _stats.AddFireRateMultiplier(value);  
                    break;
            }
        }
    }
}
