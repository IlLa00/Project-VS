using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using VS.Core;

namespace VS.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        [SerializeField] private Button pauseButton;     
        [SerializeField] private GameObject pausePanel;  
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button mainMenuButton;

        [Header("씬 이름")]
        [SerializeField] private string mainMenuSceneName = "MainMenuScene";

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
            pausePanel.SetActive(false);
            pauseButton.onClick.AddListener(OnPauseClicked);
            resumeButton.onClick.AddListener(OnResumeClicked);
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        private void OnStateChanged(GameState state)
        {
            pauseButton.gameObject.SetActive(state == GameState.Playing);

            if (state == GameState.Paused)
            {
                pausePanel.transform.DOKill();
                pausePanel.transform.localScale = Vector3.zero;
                pausePanel.SetActive(true);
                pausePanel.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
            }
            else
            {
                pausePanel.transform.DOKill();
                pausePanel.transform.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InBack).SetUpdate(true)
                    .OnComplete(() => pausePanel.SetActive(false));
            }
        }

        private void OnPauseClicked()
        {
            GameManager.Instance?.TogglePause();
        }

        private void OnResumeClicked()
        {
            GameManager.Instance?.TogglePause();
        }

        private void OnMainMenuClicked()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
