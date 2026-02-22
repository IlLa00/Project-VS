using UnityEngine;
using VS.Core;
using VS.Data;
using VS.Player;

namespace VS.UI
{
    public class LevelUpUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private UpgradeCardUI[] cards; // 카드 3개
        [SerializeField] private UpgradeData[] allUpgrades;

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
        }

        private void OnStateChanged(GameState state)
        {
            if (state == GameState.LevelUp)
                ShowCards();
            else
                panel.SetActive(false);
        }

        private void ShowCards()
        {
            panel.SetActive(true);

            UpgradeData[] chosen = PickRandom(cards.Length);
            for (int i = 0; i < cards.Length; i++)
            {
                if (i < chosen.Length)
                {
                    cards[i].gameObject.SetActive(true);
                    cards[i].Setup(chosen[i], OnCardSelected);
                }
                else
                {
                    cards[i].gameObject.SetActive(false);
                }
            }
        }

        // Fisher-Yates shuffle로 중복 없이 count개 선택
        private UpgradeData[] PickRandom(int count)
        {
            UpgradeData[] pool = (UpgradeData[])allUpgrades.Clone();
            for (int i = pool.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }
            int take = Mathf.Min(count, pool.Length);
            UpgradeData[] result = new UpgradeData[take];
            for (int i = 0; i < take; i++) result[i] = pool[i];
            return result;
        }

        private void OnCardSelected(UpgradeData upgrade)
        {
            ApplyUpgrade(upgrade);
            GameManager.Instance?.ResumePlaying();
        }

        private void ApplyUpgrade(UpgradeData upgrade)
        {
            PlayerStats stats = PlayerController.Instance?.GetComponent<PlayerStats>();
            if (stats == null) return;

            switch (upgrade.upgradeType)
            {
                case UpgradeType.DamageUp:
                    stats.AddDamageMultiplier(upgrade.value);
                    break;
                case UpgradeType.SpeedUp:
                    stats.AddMoveSpeed(upgrade.value);
                    break;
                case UpgradeType.MaxHpUp:
                    stats.AddMaxHp(upgrade.value);
                    break;
                case UpgradeType.HpRestore:
                    stats.Heal(upgrade.value);
                    break;
            }
        }
    }
}
