using TMPro;
using UnityEngine;
using VS.Core;

namespace VS.UI
{
    public class LeaderboardEntryUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text rankText;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text killText;
        [SerializeField] private TMP_Text timeText;

        public void Setup(LeaderboardEntry entry)
        {
            rankText.text = $"{entry.rank}";
            nameText.text = entry.playerName;
            killText.text = $"{entry.killCount}";

            int minutes = (int)(entry.survivalTime / 60f);
            int seconds = (int)(entry.survivalTime % 60f);
            timeText.text = $"{minutes:00}:{seconds:00}";
        }
    }
}
