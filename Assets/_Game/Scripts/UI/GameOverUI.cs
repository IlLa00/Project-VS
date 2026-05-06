using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using VS.Core;

namespace VS.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI survivalTimeText;
        [SerializeField] private TextMeshProUGUI killCountText;
        [SerializeField] private Button restartButton;

        void OnEnable()
        {
            GameManager.OnStateChanged += OnStateChanged;
        }

        void OnDisable()
        {
            GameManager.OnStateChanged -= OnStateChanged;
        }

        void Start()
        {
            panel.SetActive(false);
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        private void OnStateChanged(GameState state)
        {
            if (state != GameState.GameOver)
            {
                panel.SetActive(false);
                return;
            }

            if (Battle.BattleRoomManager.Instance != null) return;

            SaveRecords();
            ShowPanel();
        }

        private void SaveRecords()
        {
            if (GameManager.Instance != null)
            {
                float best = PlayerPrefs.GetFloat("BestTime", 0f);
                if (GameManager.Instance.SurvivalTime > best)
                {
                    PlayerPrefs.SetFloat("BestTime", GameManager.Instance.SurvivalTime);
                    PlayerPrefs.Save();
                }
            }

            KillCountManager.Instance?.SaveBestKillCount();
            SubmitToLeaderboard();
        }

        private async void SubmitToLeaderboard()
        {
            string playerName = PlayerPrefs.GetString("PlayerName", "");
            if (string.IsNullOrEmpty(playerName))
            {
                Debug.LogWarning("[Leaderboard] 닉네임 없음 — 업로드 스킵");
                return;
            }

            var lm = LeaderboardManager.Instance;
            if (lm == null)
            {
                Debug.LogWarning("[Leaderboard] LeaderboardManager 없음 — MenuScene부터 시작했는지 확인");
                return;
            }

            int killCount = PlayerPrefs.GetInt("BestKillCount", 0);
            float survivalTime = PlayerPrefs.GetFloat("BestTime", 0f);
            await lm.SubmitScore(playerName, killCount, survivalTime);
        }

        private void ShowPanel()
        {
            panel.transform.localScale = Vector3.zero;
            panel.SetActive(true);
            panel.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);

            if (survivalTimeText != null && GameManager.Instance != null)
                survivalTimeText.text = $"생존 시간\n{GameManager.Instance.GetFormattedTime()}";

            if (killCountText != null && KillCountManager.Instance != null)
                killCountText.text = $"처치 수\n{KillCountManager.Instance.KillCount}";
        }

        private void OnRestartClicked()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
