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

        [Header("씬 이름")]
        [SerializeField] private string gameSceneName = "GameScene";

        void Start()
        {
            startButton.onClick.AddListener(OnStartClicked);
            ShowBestTime();
        }

        private void OnStartClicked()
        {
            SceneManager.LoadScene(gameSceneName);
        }

        private void ShowBestTime()
        {
            float best = PlayerPrefs.GetFloat("BestTime", 0f);

            if (bestTimeText == null) return;

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
    }
}
