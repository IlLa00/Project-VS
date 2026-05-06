using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VS.Battle;

namespace VS.UI
{
    public class OpponentStatusUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text nicknameText;
        [SerializeField] private Slider hpSlider;

        void OnEnable()
        {
            BattleRoomManager.OnOpponentHpChanged += UpdateHp;
        }

        void OnDisable()
        {
            BattleRoomManager.OnOpponentHpChanged -= UpdateHp;
        }

        void Start()
        {
            if (BattleRoomManager.Instance != null && nicknameText != null)
                nicknameText.text = BattleRoomManager.Instance.OpponentNickname;
        }

        private void UpdateHp(float current, float max)
        {
            if (hpSlider != null)
                hpSlider.value = max > 0f ? current / max : 0f;
        }
    }
}
