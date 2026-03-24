using System.Collections.Generic;
using UnityEngine;
using VS.Core;
using VS.Data;
using VS.Player;

namespace VS.UI
{
    public class LevelUpUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private UpgradeCardUI[] cards;

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
                panel.SetActive(false);
        }

        private void ShowCards()
        {
            panel.SetActive(true);

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
