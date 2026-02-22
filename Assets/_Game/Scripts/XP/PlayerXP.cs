using System;
using UnityEngine;
using VS.Core;

namespace VS.Player
{
    public class PlayerXP : MonoBehaviour
    {
        public static PlayerXP Instance { get; private set; }

        [SerializeField] private float baseXpToLevel = 50f;
        [SerializeField] private float xpScaleFactor = 1.4f;

        public int Level { get; private set; } = 1;
        public float CurrentXP { get; private set; }
        public float XpToNextLevel { get; private set; }

        public event Action<float, float> OnXpChanged; // current, max
        public event Action<int> OnLevelUp;            // new level

        void Awake()
        {
            Instance = this;
            XpToNextLevel = baseXpToLevel;
        }

        public void AddXP(float amount)
        {
            CurrentXP += amount;
            OnXpChanged?.Invoke(CurrentXP, XpToNextLevel);

            if (CurrentXP >= XpToNextLevel)
                LevelUp();
        }

        private void LevelUp()
        {
            CurrentXP -= XpToNextLevel;
            Level++;
            XpToNextLevel = Mathf.Round(baseXpToLevel * Mathf.Pow(xpScaleFactor, Level - 1));
            OnLevelUp?.Invoke(Level);
            OnXpChanged?.Invoke(CurrentXP, XpToNextLevel);
            GameManager.Instance?.TriggerLevelUp();
        }
    }
}
