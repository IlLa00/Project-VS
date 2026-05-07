using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using TMPro;
using VS.Battle;
using VS.Core;
using VS.Data;
using VS.Player;

namespace VS.UI
{
    public class LevelUpUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private UpgradeCardUI[] cards;
        [SerializeField] private GameObject waitingText;

        private UpgradeDataBase[] _allUpgrades;
        private AttackCardDataBase[] _allAttackCards;
        private readonly List<ICardData> _pool = new List<ICardData>();
        private int _roundIndex;

        void Awake()
        {
            _allUpgrades = Resources.LoadAll<UpgradeDataBase>("Upgrades");
            _allAttackCards = Resources.LoadAll<AttackCardDataBase>("AttackCards");
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
            {
                if (BattleRoomManager.Instance != null)
                    BattleRoomManager.Instance.StartCardPhase(_roundIndex++);
                ShowCards();
            }
            else
            {
                HidePanel();
            }
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

            if (waitingText != null)
                waitingText.SetActive(false);

            PlayerController player = PlayerController.Instance;

            _pool.Clear();
            foreach (var u in _allUpgrades)
                if (u.IsApplicable(player))
                    _pool.Add(u);

            if (BattleRoomManager.Instance != null)
                foreach (var a in _allAttackCards)
                    _pool.Add(a);

            ICardData[] chosen = PickRandom(_pool, cards.Length);
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

        private void OnCardSelected(ICardData card)
        {
            foreach (var c in cards)
                c.SetInteractable(false);

            PlayerController player = PlayerController.Instance;

            if (card is UpgradeDataBase upgrade)
                upgrade.Apply(player);
            else if (card is AttackCardDataBase attackCard)
                BattleRoomManager.Instance?.SendAttackCard(attackCard.GetAttackType(), attackCard.GetParameters());

            if (BattleRoomManager.Instance != null)
            {
                BattleRoomManager.Instance.ReportCardSelected();
                if (waitingText != null)
                    waitingText.SetActive(true);
            }
            else
            {
                GameManager.Instance?.ResumePlaying();
            }
        }

        private ICardData[] PickRandom(List<ICardData> pool, int count)
        {
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }
            int take = Mathf.Min(count, pool.Count);
            ICardData[] result = new ICardData[take];
            for (int i = 0; i < take; i++)
                result[i] = pool[i];
            return result;
        }
    }
}
