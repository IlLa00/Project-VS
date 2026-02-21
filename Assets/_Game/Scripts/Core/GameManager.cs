using System;
using UnityEngine;

namespace VS.Core
{
    public enum GameState { Menu, Playing, LevelUp, GameOver }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; }
        public float SurvivalTime { get; private set; }

        public static event Action<GameState> OnStateChanged;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            SetState(GameState.Playing);
        }

        void Update()
        {
            if (State == GameState.Playing)
                SurvivalTime += Time.deltaTime;
        }

        public void SetState(GameState newState)
        {
            State = newState;

            // LevelUp 혹은 GameOver 상태에서는 시간 정지
            Time.timeScale = (newState == GameState.LevelUp || newState == GameState.GameOver) ? 0f : 1f;

            OnStateChanged?.Invoke(newState);
        }

        public void StartGame()
        {
            SurvivalTime = 0f;
            SetState(GameState.Playing);
        }

        public void TriggerGameOver()
        {
            SetState(GameState.GameOver);
        }

        public void TriggerLevelUp()
        {
            SetState(GameState.LevelUp);
        }

        public void ResumePlaying()
        {
            SetState(GameState.Playing);
        }

        // ex. "02:35"
        public string GetFormattedTime()
        {
            int minutes = (int)(SurvivalTime / 60f);
            int seconds = (int)(SurvivalTime % 60f);
            return $"{minutes:00}:{seconds:00}";
        }
    }
}
