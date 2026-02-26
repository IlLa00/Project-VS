using UnityEngine;
using VS.Core;
using VS.Enemies;
using VS.Player;

namespace VS.Weapons
{
    public class BeamWeapon : MonoBehaviour, IUpgradableWeapon
    {
        [Header("공격 설정")]
        [SerializeField] private float cooldown = 3f;
        [SerializeField] private float beamLength = 10f;
        [SerializeField] private float damage = 25f;

        [Header("시각 효과")]
        [SerializeField] private float beamWidth = 0.3f;
        [SerializeField] private float beamDuration = 0.2f;
        [SerializeField] private Color beamColor = new Color(0.4f, 0.8f, 1f, 1f);

        private const int MAX_UPGRADE = 5;
        private int _upgradeLevel;

        private float _timer;
        private Vector2 _lastDir = Vector2.up;

        private LineRenderer _line;
        private float _beamTimer;
        private bool _beamActive;

        private PlayerStats _playerStats;

        public int UpgradeLevel => _upgradeLevel;
        public bool CanUpgrade => _upgradeLevel < MAX_UPGRADE;

        void Awake()
        {
            _playerStats = GetComponentInParent<PlayerStats>();
            SetupLineRenderer();
        }

        void Start()
        {
            _timer = cooldown;
        }

        private void SetupLineRenderer()
        {
            _line = gameObject.AddComponent<LineRenderer>();
            _line.positionCount = 2;
            _line.startWidth = beamWidth;
            _line.endWidth = beamWidth * 0.3f;
            _line.useWorldSpace = true;
            _line.sortingLayerName = "Default";
            _line.sortingOrder = 5;
            _line.material = new Material(Shader.Find("Sprites/Default"));
            _line.startColor = beamColor;
            _line.endColor = new Color(beamColor.r, beamColor.g, beamColor.b, 0f);
            _line.enabled = false;
        }

        void Update()
        {
            if (GameManager.Instance?.State != GameState.Playing)
                return;

            if (PlayerController.Instance != null)
            {
                Vector2 dir = PlayerController.Instance.MoveDirection;
                if (dir.sqrMagnitude > 0.01f)
                    _lastDir = dir.normalized;
            }

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _timer = cooldown;
                FireBeam();
            }

            // 빔 페이드 아웃
            if (_beamActive)
            {
                _beamTimer -= Time.deltaTime;
                if (_beamTimer <= 0f)
                {
                    _beamActive = false;
                    _line.enabled = false;
                }
                else
                {
                    float alpha = _beamTimer / beamDuration;
                    _line.startColor = new Color(beamColor.r, beamColor.g, beamColor.b, alpha);
                    _line.endColor = new Color(beamColor.r, beamColor.g, beamColor.b, 0f);
                }
            }
        }

        private void FireBeam()
        {
            Vector2 origin = PlayerController.Instance != null
                ? (Vector2)PlayerController.Instance.Transform.position
                : (Vector2)transform.position;
            Vector2 fireDir = -_lastDir;
            Vector2 end = origin + fireDir * beamLength;

            float finalDamage = damage * (_playerStats?.DamageMultiplier ?? 1f);
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, fireDir, beamLength);
            foreach (var hit in hits)
            {
                EnemyBase enemy = hit.collider.GetComponent<EnemyBase>();
                enemy?.TakeDamage(finalDamage);
            }

            _line.SetPosition(0, origin);
            _line.SetPosition(1, end);
            _line.startColor = beamColor;
            _line.endColor = new Color(beamColor.r, beamColor.g, beamColor.b, 0f);
            _line.enabled = true;
            _beamTimer = beamDuration;
            _beamActive = true;
        }

        public void Upgrade()
        {
            if (!CanUpgrade)
                return;

            _upgradeLevel++;
            damage *= 1.2f;
            beamLength += 2f;
        }
    }
}
