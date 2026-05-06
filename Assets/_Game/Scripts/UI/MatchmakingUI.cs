using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VS.Battle;

namespace VS.UI
{
    public class MatchmakingUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button cancelButton;

        [Header("씬 이름")]
        [SerializeField] private string gameSceneName = "GameScene";

        void Awake()
        {
            retryButton.onClick.AddListener(OnRetryClicked);
            cancelButton.onClick.AddListener(OnCancelClicked);
            panel.SetActive(false);
        }

        void OnEnable()
        {
            WeaponSelectPanel.OnWeaponConfirmed += OnWeaponConfirmed;
            MatchmakingManager.OnMatchFound += OnMatchFound;
        }

        void OnDisable()
        {
            WeaponSelectPanel.OnWeaponConfirmed -= OnWeaponConfirmed;
            MatchmakingManager.OnMatchFound -= OnMatchFound;
        }

        private void OnWeaponConfirmed()
        {
            panel.SetActive(true);
            SetStatus("상대를 찾는 중...");
            MatchmakingManager.Instance.EnterQueue();
        }

        private void OnMatchFound()
        {
            SetStatus("매칭 성사!");
            retryButton.interactable = false;
            cancelButton.interactable = false;
            SceneManager.LoadScene(gameSceneName);
        }

        private void OnRetryClicked()
        {
            SetStatus("상대를 찾는 중...");
            MatchmakingManager.Instance.RetryMatchmaking();
        }

        private void OnCancelClicked()
        {
            MatchmakingManager.Instance.CancelMatchmaking();
            panel.SetActive(false);
        }

        private void SetStatus(string message)
        {
            if (statusText != null) statusText.text = message;
        }
    }
}
