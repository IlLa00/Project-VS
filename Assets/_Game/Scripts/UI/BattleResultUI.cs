using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VS.Battle;

namespace VS.UI
{
    public class BattleResultUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private TMP_Text opponentNicknameText;
        [SerializeField] private Button restartButton;

        void Awake()
        {
            panel.SetActive(false);
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        void OnEnable()
        {
            BattleRoomManager.OnBattleResult += ShowResult;
        }

        void OnDisable()
        {
            BattleRoomManager.OnBattleResult -= ShowResult;
        }

        private void ShowResult(bool won)
        {
            if (opponentNicknameText != null && BattleRoomManager.Instance != null)
                opponentNicknameText.text = BattleRoomManager.Instance.OpponentNickname;

            if (resultText != null)
                resultText.text = won ? "승리!" : "패배...";

            panel.transform.localScale = Vector3.zero;
            panel.SetActive(true);
            panel.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        private void OnRestartClicked()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
