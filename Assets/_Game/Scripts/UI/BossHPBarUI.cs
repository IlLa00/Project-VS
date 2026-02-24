using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VS.Enemies;

namespace VS.UI
{
    /// <summary>
    /// 보스가 스폰될 때 화면에 표시되는 전용 HP바.
    /// EnemySpawner.OnBossSpawned 이벤트를 구독해 자동으로 활성화된다.
    /// </summary>
    public class BossHPBarUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Slider hpSlider;
        [SerializeField] private TextMeshProUGUI bossNameText;

        private EnemyBase _currentBoss;

        void OnEnable()
        {
            EnemySpawner.OnBossSpawned += OnBossSpawned;
        }

        void OnDisable()
        {
            EnemySpawner.OnBossSpawned -= OnBossSpawned;
            UnsubscribeBoss();
        }

        void Start()
        {
            panel.SetActive(false);
        }

        private void OnBossSpawned(EnemyBase boss)
        {
            UnsubscribeBoss();

            _currentBoss = boss;
            _currentBoss.OnHpChanged += UpdateBar;
            _currentBoss.OnDied += HidePanel;

            if (bossNameText != null)
                bossNameText.text = boss.EnemyName;

            panel.SetActive(true);
            UpdateBar(1f, 1f); // 슬라이더 최대값으로 초기화
        }

        private void UpdateBar(float current, float max)
        {
            if (hpSlider != null)
                hpSlider.value = max > 0f ? current / max : 0f;
        }

        private void HidePanel()
        {
            panel.SetActive(false);
            UnsubscribeBoss();
        }

        private void UnsubscribeBoss()
        {
            if (_currentBoss == null) return;
            _currentBoss.OnHpChanged -= UpdateBar;
            _currentBoss.OnDied -= HidePanel;
            _currentBoss = null;
        }
    }
}
