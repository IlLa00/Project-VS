using System;
using System.Collections.Generic;
using Firebase.Database;
using UnityEngine;
using VS.Core;

namespace VS.Battle
{
    public class MatchmakingManager : MonoBehaviour
    {
        public static MatchmakingManager Instance { get; private set; }
        public static event Action OnMatchFound;

        private DatabaseReference _queueRef;
        private DatabaseReference _myQueueRef;
        private DatabaseReference _roomRef;

        private string _myUid;
        private string _myNickname;
        private string _opponentUid;
        private string _opponentNickname;
        private string _roomId;

        private bool _isSearching;
        private bool _matchConfirmed;
        private bool _pendingQueueListener;
        private bool _pendingRoomListener;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Update()
        {
            if (_pendingQueueListener)
            {
                _pendingQueueListener = false;
                _queueRef.ValueChanged += OnQueueChanged;
            }
            if (_pendingRoomListener)
            {
                _pendingRoomListener = false;
                _roomRef.ValueChanged += OnRoomChanged;
            }
        }

        public void EnterQueue()
        {
            if (_isSearching) return;
            _isSearching = true;
            _matchConfirmed = false;

            _myUid = FirebaseManager.Instance.UserId;
            _myNickname = PlayerPrefs.GetString("PlayerName", "Unknown");

            var db = FirebaseManager.Instance.Rtdb;
            _queueRef = db.GetReference("matchmaking/queue");
            _myQueueRef = _queueRef.Child(_myUid);

            var entry = new Dictionary<string, object>
            {
                { "nickname", _myNickname },
                { "timestamp", ServerValue.Timestamp }
            };

            _myQueueRef.SetValueAsync(entry).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogWarning($"[Matchmaking] queue write failed: {task.Exception?.GetBaseException().Message}");
                    return;
                }
                if (task.IsCompletedSuccessfully)
                    _pendingQueueListener = true;
            });
        }

        private void OnQueueChanged(object sender, ValueChangedEventArgs e)
        {
            if (!_isSearching || _matchConfirmed) return;
            if (e.DatabaseError != null) return;
            if (!e.Snapshot.Exists) return;

            foreach (var child in e.Snapshot.Children)
            {
                if (child.Key == _myUid) continue;

                _opponentUid = child.Key;
                _opponentNickname = child.Child("nickname").Value?.ToString() ?? "Unknown";
                HandleOpponentFound();
                return;
            }
        }

        private void HandleOpponentFound()
        {
            _queueRef.ValueChanged -= OnQueueChanged;

            _roomId = string.Compare(_myUid, _opponentUid, StringComparison.Ordinal) < 0
                ? $"{_myUid}_{_opponentUid}"
                : $"{_opponentUid}_{_myUid}";

            var db = FirebaseManager.Instance.Rtdb;
            _roomRef = db.GetReference($"rooms/{_roomId}");

            var myPlayerData = new Dictionary<string, object>
            {
                { "nickname", _myNickname },
                { "hp", 100f },
                { "maxHp", 100f },
                { "alive", true },
                { "lastHeartbeat", ServerValue.Timestamp }
            };

            _roomRef.Child("players").Child(_myUid).SetValueAsync(myPlayerData).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogWarning($"[Matchmaking] rooms write failed: {task.Exception?.GetBaseException().Message}");
                    return;
                }
                if (task.IsCompletedSuccessfully)
                    _pendingRoomListener = true;
            });
        }

        private void OnRoomChanged(object sender, ValueChangedEventArgs e)
        {
            if (_matchConfirmed) return;
            if (e.DatabaseError != null) return;
            if (!e.Snapshot.Exists) return;

            bool myEntryExists = e.Snapshot.Child("players").Child(_myUid).Exists;
            bool opponentEntryExists = e.Snapshot.Child("players").Child(_opponentUid).Exists;

            if (!myEntryExists || !opponentEntryExists) return;

            _matchConfirmed = true;
            _isSearching = false;
            _roomRef.ValueChanged -= OnRoomChanged;
            _myQueueRef.RemoveValueAsync();

            if (BattleRoomManager.Instance == null)
            {
                Debug.LogWarning("[Matchmaking] BattleRoomManager.Instance is null. Check if it's in the scene.");
                return;
            }
            BattleRoomManager.Instance.Prepare(_roomId, _myUid, _myNickname, _opponentUid, _opponentNickname);
            OnMatchFound?.Invoke();
        }

        public void CancelMatchmaking()
        {
            if (!_isSearching) return;
            _isSearching = false;
            _queueRef.ValueChanged -= OnQueueChanged;
            _roomRef.ValueChanged -= OnRoomChanged;
            _myQueueRef?.RemoveValueAsync();
        }

        public void RetryMatchmaking()
        {
            CancelMatchmaking();
            EnterQueue();
        }
    }
}
