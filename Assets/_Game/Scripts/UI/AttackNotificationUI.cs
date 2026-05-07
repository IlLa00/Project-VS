using DG.Tweening;
using TMPro;
using UnityEngine;
using VS.Battle;

namespace VS.UI
{
    public class AttackNotificationUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI notificationText;

        void OnEnable()
        {
            BattleRoomManager.OnAttackReceived += ShowNotification;
        }

        void OnDisable()
        {
            BattleRoomManager.OnAttackReceived -= ShowNotification;
        }

        private void ShowNotification(string message)
        {
            if (notificationText == null) return;

            notificationText.text = message;
            notificationText.DOKill();

            var color = notificationText.color;
            color.a = 1f;
            notificationText.color = color;

            notificationText.DOFade(0f, 1f).SetDelay(3f).SetUpdate(true);
        }
    }
}
