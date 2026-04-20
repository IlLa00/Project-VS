using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VS.Core;

namespace VS.UI
{
    public class LeaderboardPanel : MonoBehaviour
    {
        [Header("Top 10")]
        [SerializeField] private Transform entryContainer;
        [SerializeField] private LeaderboardEntryUI entryPrefab;

        [Header("내 순위")]
        [SerializeField] private GameObject myRankSection;
        [SerializeField] private LeaderboardEntryUI myEntryUI;
        [SerializeField] private TMP_Text noRecordText;

        [Header("공통")]
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject loadingIndicator;
        [SerializeField] private TMP_Text errorText;

        void Start()
        {
            closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        }

        void OnEnable()
        {
            foreach (Transform child in entryContainer)
                Destroy(child.gameObject);

            if (errorText != null) errorText.gameObject.SetActive(false);
            if (noRecordText != null) noRecordText.gameObject.SetActive(false);
            if (myEntryUI != null) myEntryUI.gameObject.SetActive(true);
            if (myRankSection != null) myRankSection.SetActive(false);

            loadingIndicator.SetActive(true);
            FetchAndDisplay();
        }

        private async void FetchAndDisplay()
        {
            if (LeaderboardManager.Instance == null)
            {
                ShowError("랭킹 서버에 연결할 수 없습니다.");
                return;
            }

            string myUid = FirebaseManager.Instance?.UserId;

            var rankingsTask = LeaderboardManager.Instance.FetchTopRankings();
            var myEntryTask = string.IsNullOrEmpty(myUid)
                ? Task.FromResult<LeaderboardEntry?>(null)
                : LeaderboardManager.Instance.FetchMyEntry(myUid);

            await Task.WhenAll(rankingsTask, myEntryTask);

            loadingIndicator.SetActive(false);

            var entries = rankingsTask.Result;
            var myEntry = myEntryTask.Result;

            if (entries == null || entries.Count == 0)
            {
                ShowError("데이터를 불러올 수 없습니다.");
            }
            else
            {
                foreach (var entry in entries)
                {
                    var item = Instantiate(entryPrefab, entryContainer);
                    item.Setup(entry);
                }
                LayoutRebuilder.ForceRebuildLayoutImmediate(entryContainer.GetComponent<RectTransform>());
            }

            if (myRankSection != null) myRankSection.SetActive(true);

            if (myEntry.HasValue)
            {
                myEntryUI.gameObject.SetActive(true);
                myEntryUI.Setup(myEntry.Value);
                if (noRecordText != null) noRecordText.gameObject.SetActive(false);
            }
            else
            {
                myEntryUI.gameObject.SetActive(false);
                if (noRecordText != null)
                {
                    noRecordText.text = "기록 없음";
                    noRecordText.gameObject.SetActive(true);
                }
            }
        }

        private void ShowError(string message)
        {
            loadingIndicator.SetActive(false);
            if (errorText != null)
            {
                errorText.text = message;
                errorText.gameObject.SetActive(true);
            }
        }
    }
}
