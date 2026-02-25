using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VS.Core;
using VS.Player;
using VS.Weapons;

namespace VS.UI
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private Slider hpSlider;
        [SerializeField] private Slider xpSlider;
        [SerializeField] private Slider cooldownSlider;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI levelText;

        private PlayerStats _playerStats;
        private PlayerXP _playerXP;
        private WeaponBase _weaponBase;

        void Start()
        {
            _playerStats = FindFirstObjectByType<PlayerStats>();
            if (_playerStats != null)
            {
                _playerStats.OnHpChanged += UpdateHpBar;
                UpdateHpBar(_playerStats.CurrentHp, _playerStats.MaxHp);
            }

            _playerXP = FindFirstObjectByType<PlayerXP>();
            if (_playerXP != null)
            {
                _playerXP.OnXpChanged += UpdateXpBar;
                _playerXP.OnLevelUp += UpdateLevelText;
                UpdateXpBar(_playerXP.CurrentXP, _playerXP.XpToNextLevel);
                UpdateLevelText(_playerXP.Level);
            }

            if (PlayerController.Instance != null)
                _weaponBase = PlayerController.Instance.GetComponentInChildren<WeaponBase>();
        }

        void OnDestroy()
        {
            if (_playerStats != null)
                _playerStats.OnHpChanged -= UpdateHpBar;

            if (_playerXP != null)
            {
                _playerXP.OnXpChanged -= UpdateXpBar;
                _playerXP.OnLevelUp -= UpdateLevelText;
            }
        }

        void Update()
        {
            if (timerText != null && GameManager.Instance != null &&
                GameManager.Instance.State == GameState.Playing)
                timerText.text = GameManager.Instance.GetFormattedTime();

            if (cooldownSlider != null && _weaponBase != null)
                cooldownSlider.value = _weaponBase.CooldownProgress;
        }

        private void UpdateHpBar(float current, float max)
        {
            if (hpSlider != null)
                hpSlider.value = current / max;
        }

        private void UpdateXpBar(float current, float max)
        {
            if (xpSlider != null)
                xpSlider.value = max > 0f ? current / max : 0f;
        }

        private void UpdateLevelText(int level)
        {
            if (levelText != null)
                levelText.text = $"Lv.{level}";
        }
    }
}
