using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VS.UI
{
    public class NicknamePopup : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private Button confirmButton;

        private const int MAX_LENGTH = 12;

        void Start()
        {
            confirmButton.onClick.AddListener(OnConfirmClicked);

            if (!string.IsNullOrEmpty(PlayerPrefs.GetString("PlayerName", "")))
                gameObject.SetActive(false);
        }

        private void OnConfirmClicked()
        {
            string input = nameInput.text.Trim();
            if (string.IsNullOrEmpty(input)) return;

            if (input.Length > MAX_LENGTH)
                input = input[..MAX_LENGTH];

            PlayerPrefs.SetString("PlayerName", input);
            PlayerPrefs.Save();
            gameObject.SetActive(false);
        }
    }
}
