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
        [SerializeField] private Slider cooldownSlider;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI killCountText;
        [SerializeField] private OpponentStatusUI opponentStatusUI;

        private PlayerStats _playerStats;
        private ProjectileWeapon _weaponBase;
        private KillCountManager _killCountManager;
        private int _lastTimerSeconds = -1;

        void Start()
        {
            _playerStats = FindFirstObjectByType<PlayerStats>();
            if (_playerStats != null)
            {
                _playerStats.OnHpChanged += UpdateHpBar;
                UpdateHpBar(_playerStats.CurrentHp, _playerStats.MaxHp);
            }

            if (PlayerController.Instance != null)
                _weaponBase = PlayerController.Instance.GetComponentInChildren<ProjectileWeapon>();

            _killCountManager = KillCountManager.Instance;
            if (_killCountManager != null)
            {
                _killCountManager.OnKillCountChanged += UpdateKillCountText;
                UpdateKillCountText(0);
            }

            if (opponentStatusUI != null)
                opponentStatusUI.gameObject.SetActive(Battle.BattleRoomManager.Instance != null);

            AdManager.Instance?.ShowBanner();
        }

        void OnDestroy()
        {
            if (_playerStats != null)
                _playerStats.OnHpChanged -= UpdateHpBar;

            if (_killCountManager != null)
                _killCountManager.OnKillCountChanged -= UpdateKillCountText;
        }

        void Update()
        {
            if (timerText != null && GameManager.Instance != null &&
                GameManager.Instance.State == GameState.Playing)
            {
                int currentSeconds = (int)GameManager.Instance.SurvivalTime;
                if (currentSeconds != _lastTimerSeconds)
                {
                    _lastTimerSeconds = currentSeconds;
                    timerText.text = GameManager.Instance.GetFormattedTime();
                }
            }

            if (cooldownSlider != null && _weaponBase != null)
                cooldownSlider.value = _weaponBase.CooldownProgress;
        }

        private void UpdateHpBar(float current, float max)
        {
            if (hpSlider != null)
                hpSlider.value = current / max;
        }

        private void UpdateKillCountText(int count)
        {
            if (killCountText != null)
                killCountText.text = "처치 " + count;
        }
    }
}
