using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VS.Core;

namespace VS.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private TextMeshProUGUI bestTimeText;
        [SerializeField] private TextMeshProUGUI bestKillCountText;
        [SerializeField] private GameObject weaponSelectPanel;
        [SerializeField] private Button leaderboardButton;
        [SerializeField] private GameObject leaderboardPanel;

        void Start()
        {
            startButton.onClick.AddListener(OnStartClicked);
            leaderboardButton.onClick.AddListener(() => leaderboardPanel.SetActive(true));
            ShowBestTime();
            ShowBestKillCount();
            SoundManager.Instance?.PlayMainMenuBGM();
        }

        private void OnStartClicked()
        {
            weaponSelectPanel.SetActive(true);
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
