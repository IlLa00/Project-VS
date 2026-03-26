using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace VS.Core
{
    public class ClaudeAdvisorService : MonoBehaviour
    {
        public static ClaudeAdvisorService Instance { get; private set; }

        [SerializeField] private string apiKey;
        [SerializeField] private string model = "claude-haiku-4-5";
        [SerializeField] private float timeoutSeconds = 5f;

        private const string ApiUrl = "https://api.anthropic.com/v1/messages";
        private const string SystemPrompt =
            "너는 뱀서라이크 게임의 AI 조언자야. " +
            "플레이어 상황을 보고 업그레이드 선택지 중 하나를 추천해줘. " +
            "한국어로 1문장 이내로, 업그레이드 이름을 직접 언급해서 답해줘.";

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void RequestAdvice(string gameContext, Action<string> onResult)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                Debug.LogWarning("[ClaudeAdvisor] API 키가 비어있어 요청을 건너뜁니다.");
                onResult?.Invoke(null);
                return;
            }
            Debug.Log($"[ClaudeAdvisor] 요청 시작\n{gameContext}");
            StartCoroutine(CallApi(gameContext, onResult));
        }

        private IEnumerator CallApi(string gameContext, Action<string> onResult)
        {
            string body = BuildRequestBody(gameContext);
            byte[] bodyBytes = Encoding.UTF8.GetBytes(body);

            using var request = new UnityWebRequest(ApiUrl, "POST");
            request.uploadHandler = new UploadHandlerRaw(bodyBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("content-type", "application/json");
            request.SetRequestHeader("x-api-key", apiKey);
            request.SetRequestHeader("anthropic-version", "2023-06-01");
            request.timeout = (int)timeoutSeconds;

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[ClaudeAdvisor] 요청 실패 — {request.result} / HTTP {request.responseCode}\n{request.error}\n응답 바디: {request.downloadHandler.text}");
                onResult?.Invoke(null);
                yield break;
            }

            Debug.Log($"[ClaudeAdvisor] 응답 수신 (HTTP {request.responseCode})\n{request.downloadHandler.text}");
            string text = ParseResponse(request.downloadHandler.text);

            if (text == null)
                Debug.LogWarning("[ClaudeAdvisor] 응답 파싱 실패 — text 필드를 찾지 못했습니다.");
            else
                Debug.Log($"[ClaudeAdvisor] 파싱된 조언: {text}");

            onResult?.Invoke(text);
        }

        private string BuildRequestBody(string userContent)
        {
            return $@"{{
                ""model"": ""{model}"",
                ""max_tokens"": 150,
                ""system"": ""{EscapeJson(SystemPrompt)}"",
                ""messages"": [
                    {{ ""role"": ""user"", ""content"": ""{EscapeJson(userContent)}"" }}
                ]
                }}";
        }

        private static string ParseResponse(string json)
        {
            const string marker = "\"text\":\"";
            int start = json.IndexOf(marker, StringComparison.Ordinal);
            if (start < 0) return null;
            start += marker.Length;
            int end = json.IndexOf("\"", start, StringComparison.Ordinal);
            if (end < 0) return null;
            return json.Substring(start, end - start).Replace("\\n", "\n");
        }

        private static string EscapeJson(string s) =>
            s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n");
    }
}
