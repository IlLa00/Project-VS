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

            panel.SetActive(true);

            if (survivalTimeText != null && GameManager.Instance != null)
                survivalTimeText.text = $"생존 시간\n{GameManager.Instance.GetFormattedTime()}";

            // 최고 기록 저장
            if (GameManager.Instance != null)
            {
                float best = PlayerPrefs.GetFloat("BestTime", 0f);
                if (GameManager.Instance.SurvivalTime > best)
                {
                    PlayerPrefs.SetFloat("BestTime", GameManager.Instance.SurvivalTime);
                    PlayerPrefs.Save();
                }
            }
        }

        private void OnRestartClicked()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
