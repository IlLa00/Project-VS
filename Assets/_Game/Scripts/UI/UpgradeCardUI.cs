using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VS.Data;

namespace VS.UI
{
    public class UpgradeCardUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Button button;
        [SerializeField] private GameObject attackBadge;

        private ICardData _data;
        private Action<ICardData> _onSelected;

        public void Setup(ICardData data, Action<ICardData> onSelected)
        {
            _data = data;
            _onSelected = onSelected;

            nameText.text = data.CardName;
            descriptionText.text = data.Description;

            if (iconImage != null)
                iconImage.sprite = data.Icon;

            if (attackBadge != null)
                attackBadge.SetActive(data is AttackCardDataBase);

            button.interactable = true;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => _onSelected?.Invoke(_data));
        }

        public void SetInteractable(bool value)
        {
            button.interactable = value;
        }
    }
}
