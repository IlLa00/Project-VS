using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Database;
using UnityEngine;
using UnityEngine.SceneManagement;
using VS.Core;
using VS.Player;

namespace VS.Battle
{
    public class BattleRoomManager : MonoBehaviour
    {
        public static BattleRoomManager Instance { get; private set; }

        public static event Action<float, float> OnOpponentHpChanged;
        public static event Action<bool> OnBattleResult;

        private string _roomId;
        private string _myUid;
        private string _myNickname;
        private string _opponentUid;
        private string _opponentNickname;

        private DatabaseReference _myPlayerRef;
        private DatabaseReference _opponentPlayerRef;
        private DatabaseReference _attacksRef;

        private bool _isPrepared;
        private bool _battleActive;
        private bool _resultShown;

        private float _lastOpponentHeartbeat;
        private bool _myDeathReported;

        public string OpponentNickname => _opponentNickname;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void Prepare(string roomId, string myUid, string myNickname, string opponentUid, string opponentNickname)
        {
            _roomId = roomId;
            _myUid = myUid;
            _myNickname = myNickname;
            _opponentUid = opponentUid;
            _opponentNickname = opponentNickname;
            _isPrepared = true;
            _battleActive = false;
            _resultShown = false;
            _myDeathReported = false;

            var db = FirebaseManager.Instance.Rtdb;
            _myPlayerRef = db.GetReference($"rooms/{_roomId}/players/{_myUid}");
            _opponentPlayerRef = db.GetReference($"rooms/{_roomId}/players/{_opponentUid}");
            _attacksRef = db.GetReference($"rooms/{_roomId}/attacks");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!_isPrepared) return;
            StartCoroutine(InitBattleAfterFrame());
        }

        private IEnumerator InitBattleAfterFrame()
        {
            yield return null;

            var stats = FindFirstObjectByType<PlayerStats>();
            if (stats != null)
            {
                stats.OnHpChanged += OnMyHpChanged;
                UpdateMyHp(stats.CurrentHp, stats.MaxHp);
            }

            _lastOpponentHeartbeat = Time.time;
            _battleActive = true;

            StartCoroutine(HeartbeatCoroutine());
            StartCoroutine(DisconnectCheckCoroutine());
            _opponentPlayerRef.ValueChanged += OnOpponentStatusChanged;
        }

        private IEnumerator HeartbeatCoroutine()
        {
            while (_battleActive)
            {
                _myPlayerRef.Child("lastHeartbeat").SetValueAsync(ServerValue.Timestamp);
                yield return new WaitForSeconds(2f);
            }
        }

        private IEnumerator DisconnectCheckCoroutine()
        {
            while (_battleActive)
            {
                yield return new WaitForSeconds(1f);
                if (Time.time - _lastOpponentHeartbeat > 5f)
                {
                    HandleResult(true);
                }
            }
        }

        private void OnOpponentStatusChanged(object sender, ValueChangedEventArgs e)
        {
            if (!_battleActive || _resultShown) return;
            if (e.DatabaseError != null) return;
            if (!e.Snapshot.Exists) return;

            _lastOpponentHeartbeat = Time.time;

            var hpVal = e.Snapshot.Child("hp").Value;
            var maxHpVal = e.Snapshot.Child("maxHp").Value;
            if (hpVal != null && maxHpVal != null)
            {
                float hp = Convert.ToSingle(hpVal);
                float maxHp = Convert.ToSingle(maxHpVal);
                OnOpponentHpChanged?.Invoke(hp, maxHp);
            }

            var aliveVal = e.Snapshot.Child("alive").Value;
            if (aliveVal != null && !(bool)aliveVal)
                DetermineResult();
        }

        private void DetermineResult()
        {
            if (_myDeathReported)
            {
                _myPlayerRef.Child("deathTimestamp").GetValueAsync().ContinueWith(myTask =>
                {
                    _opponentPlayerRef.Child("deathTimestamp").GetValueAsync().ContinueWith(oppTask =>
                    {
                        long myTs = myTask.IsCompletedSuccessfully && myTask.Result.Exists
                            ? Convert.ToInt64(myTask.Result.Value) : long.MaxValue;
                        long oppTs = oppTask.IsCompletedSuccessfully && oppTask.Result.Exists
                            ? Convert.ToInt64(oppTask.Result.Value) : long.MaxValue;

                        HandleResult(myTs > oppTs);
                    });
                });
            }
            else
            {
                HandleResult(true);
            }
        }

        private void HandleResult(bool won)
        {
            if (_resultShown) return;
            _resultShown = true;
            _battleActive = false;

            _opponentPlayerRef.ValueChanged -= OnOpponentStatusChanged;

            FirebaseDatabase.DefaultInstance
                .GetReference($"rooms/{_roomId}/winner")
                .SetValueAsync(won ? _myUid : _opponentUid);

            OnBattleResult?.Invoke(won);
            GameManager.Instance?.SetState(GameState.GameOver);
        }

        public void ReportMyDeath()
        {
            if (_myDeathReported) return;
            _myDeathReported = true;

            var data = new Dictionary<string, object>
            {
                { "alive", false },
                { "deathTimestamp", ServerValue.Timestamp }
            };
            _myPlayerRef.UpdateChildrenAsync(data);

            HandleResult(false);
        }

        public void SendAttackCard(string type, Dictionary<string, object> parameters)
        {
            if (!_battleActive) return;

            var attackData = new Dictionary<string, object>(parameters)
            {
                { "type", type },
                { "timestamp", ServerValue.Timestamp }
            };
            _attacksRef.Push().SetValueAsync(attackData);
        }

        private void OnMyHpChanged(float current, float max)
        {
            UpdateMyHp(current, max);
        }

        private void UpdateMyHp(float current, float max)
        {
            var data = new Dictionary<string, object>
            {
                { "hp", current },
                { "maxHp", max }
            };
            _myPlayerRef.UpdateChildrenAsync(data);
        }
    }
}
