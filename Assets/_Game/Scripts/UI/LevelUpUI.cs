using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using UnityEngine;
using TMPro;
using VS.Core;
using VS.Data;
using VS.Player;
using VS.Weapons;

namespace VS.UI
{
    public class LevelUpUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private UpgradeCardUI[] cards;
        [SerializeField] private TextMeshProUGUI adviceText;

        private UpgradeDataBase[] _allUpgrades;
        private readonly List<UpgradeDataBase> _applicableBuffer = new List<UpgradeDataBase>();

        void Awake()
        {
            _allUpgrades = Resources.LoadAll<UpgradeDataBase>("Upgrades");
        }

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
                HidePanel();
        }

        private void HidePanel()
        {
            panel.transform.DOKill();
            panel.transform.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InBack).SetUpdate(true)
                .OnComplete(() => panel.SetActive(false));
        }

        private void ShowCards()
        {
            panel.transform.DOKill();
            panel.transform.localScale = Vector3.zero;
            panel.SetActive(true);
            panel.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);

            PlayerController player = PlayerController.Instance;
            _applicableBuffer.Clear();
            foreach (var u in _allUpgrades)
                if (u.IsApplicable(player))
                    _applicableBuffer.Add(u);

            UpgradeDataBase[] chosen = PickRandom(_applicableBuffer, cards.Length);
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

            RequestAIAdvice(player, chosen);
        }

        private void RequestAIAdvice(PlayerController player, UpgradeDataBase[] chosen)
        {
            if (adviceText == null || ClaudeAdvisorService.Instance == null) return;

            adviceText.text = "분석 중...";

            string context = BuildContext(player, chosen);
            ClaudeAdvisorService.Instance.RequestAdvice(context, result =>
            {
                if (adviceText == null) return;
                adviceText.text = result ?? string.Empty;
            });
        }

        private static string BuildContext(PlayerController player, UpgradeDataBase[] chosen)
        {
            var stats = player.GetComponent<PlayerStats>();
            var inventory = player.GetComponent<WeaponInventory>();

            var sb = new StringBuilder();
            sb.Append("현재 상황: 레벨 ");
            sb.Append(PlayerXP.Instance != null ? PlayerXP.Instance.Level : 1);
            sb.Append(", 생존 ");
            sb.Append(GameManager.Instance != null ? GameManager.Instance.GetFormattedTime() : "00:00");

            if (stats != null)
            {
                sb.Append(", HP ");
                sb.Append(Mathf.RoundToInt(stats.CurrentHp));
                sb.Append("/");
                sb.Append(Mathf.RoundToInt(stats.MaxHp));
            }

            if (inventory != null && inventory.Weapons.Count > 0)
            {
                sb.Append("\n장착 무기: ");
                for (int i = 0; i < inventory.Weapons.Count; i++)
                {
                    if (i > 0) sb.Append(", ");
                    sb.Append(inventory.Weapons[i].Data.upgradeName);
                }
            }

            sb.Append("\n업그레이드 선택지:");
            for (int i = 0; i < chosen.Length; i++)
            {
                sb.Append("\n");
                sb.Append(i + 1);
                sb.Append(". ");
                sb.Append(chosen[i].upgradeName);
                sb.Append(": ");
                sb.Append(chosen[i].description);
            }

            return sb.ToString();
        }

        private UpgradeDataBase[] PickRandom(List<UpgradeDataBase> pool, int count)
        {
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }
            int take = Mathf.Min(count, pool.Count);
            UpgradeDataBase[] result = new UpgradeDataBase[take];
            for (int i = 0; i < take; i++)
                result[i] = pool[i];
            return result;
        }

        private void OnCardSelected(UpgradeDataBase upgrade)
        {
            PlayerController player = PlayerController.Instance;
            if (player != null)
                upgrade.Apply(player);

            GameManager.Instance?.ResumePlaying();
        }
    }
}
