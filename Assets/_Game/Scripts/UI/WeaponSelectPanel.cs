using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using VS.Core;
using VS.Data;

namespace VS.UI
{
    public class WeaponSelectPanel : MonoBehaviour
    {
        [SerializeField] private WeaponAcquireData[] selectableWeapons;
        [SerializeField] private Transform slotContainer;
        [SerializeField] private WeaponSelectSlotUI slotPrefab;
        [SerializeField] private Button confirmButton;

        [Header("씬 이름")]
        [SerializeField] private string gameSceneName = "GameScene";

        private WeaponSelectSlotUI _selectedSlot;

        void Awake()
        {
            confirmButton.onClick.AddListener(OnConfirm);

            foreach (var data in selectableWeapons)
            {
                var slot = Instantiate(slotPrefab, slotContainer);
                slot.Init(data, OnSlotSelected);
            }
        }

        void OnEnable()
        {
            _selectedSlot = null;

            slotContainer.localScale = Vector3.zero;
            confirmButton.transform.localScale = Vector3.zero;

            slotContainer.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
            confirmButton.transform.DOScale(1f, 0.35f).SetDelay(0.1f).SetEase(Ease.OutBack);
        }

        private void OnSlotSelected(WeaponSelectSlotUI slot)
        {
            if (_selectedSlot != null)
                _selectedSlot.SetHighlight(false);

            _selectedSlot = slot;
            _selectedSlot.SetHighlight(true);
        }

        private void OnConfirm()
        {
            if (_selectedSlot == null) return;

            confirmButton.interactable = false;
            confirmButton.transform
                .DOPunchScale(Vector3.one * 0.3f, 0.4f, 5, 0.5f)
                .OnComplete(() =>
                {
                    WeaponSelectManager.Select(_selectedSlot.Data);
                    SceneManager.LoadScene(gameSceneName);
                });
        }
    }
}
