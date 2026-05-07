using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Database;
using UnityEngine;
using UnityEngine.SceneManagement;
using VS.Core;
using VS.Enemies;
using VS.Player;
using VS.Weapons;

namespace VS.Battle
{
    public class BattleRoomManager : MonoBehaviour
    {
        public static BattleRoomManager Instance { get; private set; }

        public static event Action<float, float> OnOpponentHpChanged;
        public static event Action<bool> OnBattleResult;
        public static event Action<string> OnAttackReceived;

        private string _roomId;
        private string _myUid;
        private string _myNickname;
        private string _opponentUid;
        private string _opponentNickname;

        private DatabaseReference _myPlayerRef;
        private DatabaseReference _opponentPlayerRef;
        private DatabaseReference _attacksRef;
        private DatabaseReference _wavesRef;
        private DatabaseReference _cardPhaseRef;

        private bool _isPrepared;
        private bool _battleActive;
        private bool _resultShown;

        private float _lastOpponentHeartbeat;
        private bool _myDeathReported;

        private int _myKillCount;
        private int _cardRoundIndex;
        private bool _myCardReady;

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
            _myKillCount = 0;
            _cardRoundIndex = 0;
            _myCardReady = false;

            var db = FirebaseManager.Instance.Rtdb;
            _myPlayerRef = db.GetReference($"rooms/{_roomId}/players/{_myUid}");
            _opponentPlayerRef = db.GetReference($"rooms/{_roomId}/players/{_opponentUid}");
            _attacksRef = db.GetReference($"rooms/{_roomId}/attacks");
            _wavesRef = db.GetReference($"rooms/{_roomId}/waves");
            _cardPhaseRef = db.GetReference($"rooms/{_roomId}/cardPhase");
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
            _attacksRef.ChildAdded += OnAttackChildAdded;
            _wavesRef.ChildAdded += OnWaveChildAdded;
            _cardPhaseRef.ValueChanged += OnCardPhaseChanged;
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
                    HandleResult(true);
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

        private void OnAttackChildAdded(object sender, ChildChangedEventArgs e)
        {
            if (!_battleActive || e.DatabaseError != null || !e.Snapshot.Exists) return;

            string type = e.Snapshot.Child("type").Value as string;
            if (string.IsNullOrEmpty(type)) return;

            UnityMainThreadDispatcher.Enqueue(() =>
            {
                if (type == "spawn_surge")
                {
                    float multiplier = Convert.ToSingle(e.Snapshot.Child("multiplier").Value ?? 3f);
                    float duration = Convert.ToSingle(e.Snapshot.Child("duration").Value ?? 10f);
                    EnemySpawner.Instance?.ApplySpawnSurge(multiplier, duration);
                    OnAttackReceived?.Invoke("스폰 증가 공격을 받았습니다!");
                }
                else if (type == "weapon_downgrade")
                {
                    var inventory = FindFirstObjectByType<WeaponInventory>();
                    inventory?.DowngradeRandomWeapon();
                    OnAttackReceived?.Invoke("무기 다운그레이드 공격을 받았습니다!");
                }
            });
        }

        private void OnWaveChildAdded(object sender, ChildChangedEventArgs e)
        {
            if (!_battleActive || e.DatabaseError != null || !e.Snapshot.Exists) return;

            int count = Convert.ToInt32(e.Snapshot.Child("count").Value ?? 5);
            UnityMainThreadDispatcher.Enqueue(() => StartCoroutine(ForceSpawnAfterDelay(count)));
        }

        private IEnumerator ForceSpawnAfterDelay(int count)
        {
            yield return new WaitForSeconds(0.5f);
            EnemySpawner.Instance?.ForceSpawn(count);
        }

        private void OnCardPhaseChanged(object sender, ValueChangedEventArgs e)
        {
            if (!_battleActive || e.DatabaseError != null || !e.Snapshot.Exists) return;

            var opponentReady = e.Snapshot.Child(_opponentUid).Child("ready").Value;
            if (opponentReady != null && (bool)opponentReady && _myCardReady)
            {
                UnityMainThreadDispatcher.Enqueue(() => GameManager.Instance?.ResumePlaying());
            }
        }

        public void ReportKill()
        {
            if (!_battleActive) return;
            _myKillCount++;
            if (_myKillCount % 5 == 0)
            {
                var waveData = new Dictionary<string, object>
                {
                    { "count", 5 },
                    { "timestamp", ServerValue.Timestamp }
                };
                _wavesRef.Push().SetValueAsync(waveData);
            }
        }

        public void StartCardPhase(int roundIndex)
        {
            if (!_battleActive) return;
            _myCardReady = false;
            _cardPhaseRef.Child("roundIndex").SetValueAsync(roundIndex);
            _cardPhaseRef.Child(_myUid).Child("ready").SetValueAsync(false);
        }

        public void ReportCardSelected()
        {
            if (!_battleActive) return;
            _myCardReady = true;
            _cardPhaseRef.Child(_myUid).Child("ready").SetValueAsync(true);
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
            _attacksRef.ChildAdded -= OnAttackChildAdded;
            _wavesRef.ChildAdded -= OnWaveChildAdded;
            _cardPhaseRef.ValueChanged -= OnCardPhaseChanged;

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
