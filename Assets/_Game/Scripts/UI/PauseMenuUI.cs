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
            pausePanel.SetActive(state == GameState.Paused);
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
