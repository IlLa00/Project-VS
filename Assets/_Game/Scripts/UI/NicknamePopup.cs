using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using VS.Core;

namespace VS.UI
{
    public class NicknamePopup : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private Button confirmButton;
        [SerializeField] private TMP_Text errorText;

        private const int MAX_LENGTH = 12;

        private bool _checkDone;
        private bool _nicknameAvailable;
        private string _pendingNickname;

        void Start()
        {
            confirmButton.onClick.AddListener(OnConfirmClicked);
            if (errorText != null) errorText.gameObject.SetActive(false);

            if (!string.IsNullOrEmpty(PlayerPrefs.GetString("PlayerName", "")))
                gameObject.SetActive(false);
        }

        void Update()
        {
            if (!_checkDone) return;
            _checkDone = false;

            if (_nicknameAvailable)
                ApplyNickname(_pendingNickname);
            else
                ShowError("이미 사용 중인 닉네임입니다.");
        }

        private void OnConfirmClicked()
        {
            string input = nameInput.text.Trim();
            if (string.IsNullOrEmpty(input)) return;
            if (input.Length > MAX_LENGTH) input = input[..MAX_LENGTH];

            if (errorText != null) errorText.gameObject.SetActive(false);
            confirmButton.interactable = false;

            if (!FirebaseManager.Instance.IsReady)
            {
                ShowError("서버 연결 중입니다. 잠시 후 다시 시도해주세요.");
                confirmButton.interactable = true;
                return;
            }

            _pendingNickname = input;
            CheckNicknameUnique(input);
        }

        private void CheckNicknameUnique(string nickname)
        {
            FirebaseManager.Instance.Db
                .Collection("nicknames")
                .Document(nickname)
                .GetSnapshotAsync()
                .ContinueWith(task =>
                {
                    _nicknameAvailable = task.IsCompletedSuccessfully && !task.Result.Exists;
                    _checkDone = true;
                });
        }

        private void ApplyNickname(string nickname)
        {
            var data = new System.Collections.Generic.Dictionary<string, object>
            {
                { "uid", FirebaseManager.Instance.UserId },
                { "createdAt", FieldValue.ServerTimestamp }
            };

            FirebaseManager.Instance.Db
                .Collection("nicknames")
                .Document(nickname)
                .SetAsync(data);

            PlayerPrefs.SetString("PlayerName", nickname);
            PlayerPrefs.Save();
            confirmButton.interactable = true;
            gameObject.SetActive(false);
        }

        private void ShowError(string message)
        {
            confirmButton.interactable = true;
            if (errorText == null) return;
            errorText.text = message;
            errorText.gameObject.SetActive(true);
        }
    }
}
