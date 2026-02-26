using System;
using UnityEngine;
using VS.Enemies;

namespace VS.Core
{
    public class KillCountManager : MonoBehaviour
    {
        public static KillCountManager Instance { get; private set; }

        private const string KeyBestKillCount = "BestKillCount";

        public int KillCount { get; private set; }
        public int BestKillCount => PlayerPrefs.GetInt(KeyBestKillCount, 0);

        public event Action<int> OnKillCountChanged;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnEnable()
        {
            EnemyBase.OnAnyEnemyDied += HandleEnemyDied;
        }

        void OnDisable()
        {
            EnemyBase.OnAnyEnemyDied -= HandleEnemyDied;
        }

        private void HandleEnemyDied()
        {
            KillCount++;
            OnKillCountChanged?.Invoke(KillCount);
        }

        public void SaveBestKillCount()
        {
            if (KillCount > BestKillCount)
            {
                PlayerPrefs.SetInt(KeyBestKillCount, KillCount);
                PlayerPrefs.Save();
            }
        }
    }
}
