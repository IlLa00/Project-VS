using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace VS.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private TextMeshProUGUI bestTimeText;
        [SerializeField] private TextMeshProUGUI bestKillCountText;

        [Header("씬 이름")]
        [SerializeField] private string gameSceneName = "GameScene";

        void Start()
        {
            startButton.onClick.AddListener(OnStartClicked);
            ShowBestTime();
            ShowBestKillCount();
        }

        private void OnStartClicked()
        {
            SceneManager.LoadScene(gameSceneName);
        }

        private void ShowBestTime()
        {
            if (bestTimeText == null) return;

            float best = PlayerPrefs.GetFloat("BestTime", 0f);
            if (best <= 0f)
            {
                bestTimeText.text = "최고 기록: -";
            }
            else
            {
                int minutes = (int)(best / 60f);
                int seconds = (int)(best % 60f);
                bestTimeText.text = $"최고 기록: {minutes:00}:{seconds:00}";
            }
        }

        private void ShowBestKillCount()
        {
            if (bestKillCountText == null) return;

            int best = PlayerPrefs.GetInt("BestKillCount", 0);
            bestKillCountText.text = best <= 0 ? "최다 처치: -" : $"최다 처치: {best}";
        }
    }
}
