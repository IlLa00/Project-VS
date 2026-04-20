using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;

namespace VS.Core
{
    public struct LeaderboardEntry
    {
        public int rank;
        public string uid;
        public string playerName;
        public int killCount;
        public float survivalTime;
    }

    public class LeaderboardManager : MonoBehaviour
    {
        public static LeaderboardManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public async Task SubmitScore(string playerName, int killCount, float survivalTime)
        {
            var fm = FirebaseManager.Instance;
            if (fm == null) { Debug.LogWarning("[Leaderboard] FirebaseManager 없음 — MenuScene부터 시작했는지 확인"); return; }

            float waited = 0f;
            while (!fm.IsReady && waited < 5f)
            {
                await Task.Delay(200);
                waited += 0.2f;
            }

            if (!fm.IsReady) { Debug.LogWarning("[Leaderboard] Firebase 초기화 타임아웃 — 업로드 스킵"); return; }

            var docRef = fm.Db.Collection("leaderboard").Document(fm.UserId);
            try
            {
                await docRef.SetAsync(new Dictionary<string, object>
                {
                    { "playerName", playerName },
                    { "killCount", killCount },
                    { "survivalTime", survivalTime },
                    { "updatedAt", FieldValue.ServerTimestamp }
                });
                Debug.Log($"[Leaderboard] 업로드 성공 — {playerName} kill:{killCount}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Leaderboard] SubmitScore failed: {e.Message}");
            }
        }

        public async Task<List<LeaderboardEntry>> FetchTopRankings()
        {
            var fm = FirebaseManager.Instance;
            if (fm == null) return new List<LeaderboardEntry>();

            float waited = 0f;
            while (!fm.IsReady && waited < 5f)
            {
                await Task.Delay(200);
                waited += 0.2f;
            }

            if (!fm.IsReady) return new List<LeaderboardEntry>();

            try
            {
                var snapshot = await fm.Db.Collection("leaderboard")
                    .OrderByDescending("killCount")
                    .Limit(10)
                    .GetSnapshotAsync();

                var entries = new List<LeaderboardEntry>();
                int rank = 1;
                foreach (var doc in snapshot.Documents)
                {
                    entries.Add(new LeaderboardEntry
                    {
                        rank = rank++,
                        uid = doc.Id,
                        playerName = doc.GetValue<string>("playerName"),
                        killCount = doc.GetValue<int>("killCount"),
                        survivalTime = doc.GetValue<float>("survivalTime")
                    });
                }
                return entries;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Leaderboard] FetchTopRankings failed: {e.Message}");
                return new List<LeaderboardEntry>();
            }
        }

        public async Task<LeaderboardEntry?> FetchMyEntry(string uid)
        {
            var fm = FirebaseManager.Instance;
            if (fm == null) return null;

            float waited = 0f;
            while (!fm.IsReady && waited < 5f)
            {
                await Task.Delay(200);
                waited += 0.2f;
            }

            if (!fm.IsReady) return null;

            try
            {
                var myDoc = await fm.Db.Collection("leaderboard").Document(uid).GetSnapshotAsync();
                if (!myDoc.Exists) return null;

                int myKillCount = myDoc.GetValue<int>("killCount");
                float mySurvivalTime = myDoc.GetValue<float>("survivalTime");
                string myPlayerName = myDoc.GetValue<string>("playerName");

                // 내 킬 카운트보다 높은 문서 수 = 내 순위 - 1
                var higherSnapshot = await fm.Db.Collection("leaderboard")
                    .WhereGreaterThan("killCount", myKillCount)
                    .GetSnapshotAsync();

                return new LeaderboardEntry
                {
                    rank = higherSnapshot.Count + 1,
                    uid = uid,
                    playerName = myPlayerName,
                    killCount = myKillCount,
                    survivalTime = mySurvivalTime
                };
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Leaderboard] FetchMyEntry failed: {e.Message}");
                return null;
            }
        }
    }
}
