using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VS.Core;
using VS.Player;

namespace VS.UI
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private Slider hpSlider;
        [SerializeField] private TextMeshProUGUI timerText;

        private PlayerStats _playerStats;

        void Start()
        {
            _playerStats = FindFirstObjectByType<PlayerStats>();
            if (_playerStats != null)
            {
                _playerStats.OnHpChanged += UpdateHpBar;
                UpdateHpBar(_playerStats.CurrentHp, _playerStats.MaxHp);
            }
        }

        void OnDestroy()
        {
            if (_playerStats != null)
                _playerStats.OnHpChanged -= UpdateHpBar;
        }

        void Update()
        {
            if (timerText != null && GameManager.Instance != null &&
                GameManager.Instance.State == GameState.Playing)
                timerText.text = GameManager.Instance.GetFormattedTime();
        }

        private void UpdateHpBar(float current, float max)
        {
            if (hpSlider != null)
                hpSlider.value = current / max;
        }
    }
}
