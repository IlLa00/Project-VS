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

        private UpgradeData _data;
        private Action<UpgradeData> _onSelected;

        public void Setup(UpgradeData data, Action<UpgradeData> onSelected)
        {
            _data = data;
            _onSelected = onSelected;

            nameText.text = data.upgradeName;
            descriptionText.text = data.description;

            if (iconImage != null)
                iconImage.sprite = data.icon;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => _onSelected?.Invoke(_data));
        }
    }
}
