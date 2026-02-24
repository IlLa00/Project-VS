using UnityEngine;
using VS.Core;
using VS.Data;
using VS.Player;

namespace VS.UI
{
    public class LevelUpUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private UpgradeCardUI[] cards; // 카드 슬롯 (Inspector에서 3개 연결)

        private UpgradeDataBase[] _allUpgrades;

        void Awake()
        {
            // Assets/_Game/Resources/Upgrades/ 폴더의 에셋을 자동으로 전부 로드
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
                panel.SetActive(false);
        }

        private void ShowCards()
        {
            panel.SetActive(true);

            UpgradeDataBase[] chosen = PickRandom(cards.Length);
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
        private UpgradeDataBase[] PickRandom(int count)
        {
            UpgradeDataBase[] pool = (UpgradeDataBase[])_allUpgrades.Clone();
            for (int i = pool.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }
            int take = Mathf.Min(count, pool.Length);
            UpgradeDataBase[] result = new UpgradeDataBase[take];
            for (int i = 0; i < take; i++) result[i] = pool[i];
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
