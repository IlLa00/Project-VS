using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VS.Data;

namespace VS.UI
{
    public class WeaponSelectSlotUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image highlightBorder;
        [SerializeField] private Button button;

        private WeaponAcquireData _data;
        private Action<WeaponSelectSlotUI> _onSelected;

        public WeaponAcquireData Data => _data;

        public void Init(WeaponAcquireData data, Action<WeaponSelectSlotUI> onSelected)
        {
            _data = data;
            _onSelected = onSelected;

            iconImage.sprite = data.icon;
            iconImage.enabled = data.icon != null;

            SetHighlight(false);
            button.onClick.AddListener(OnClicked);
        }

        private void OnClicked()
        {
            iconImage.transform
                .DOPunchScale(Vector3.one * 0.25f, 0.3f, 5, 0.5f)
                .OnComplete(() => _onSelected?.Invoke(this));
        }

        public void SetHighlight(bool on)
        {
            if (highlightBorder != null)
                highlightBorder.enabled = on;
        }
    }
}
