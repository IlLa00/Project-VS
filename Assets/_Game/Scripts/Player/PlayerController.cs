using UnityEngine;
using VS.Core;

namespace VS.Player
{
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("걷기 애니메이션")]
        [SerializeField] private Sprite[] walkFrames;
        [SerializeField] private float animFrameRate = 8f;

        [Header("사망 애니메이션")]
        [SerializeField] private Sprite[] deathFrames;
        [SerializeField] private float deathFrameRate = 6f;

        private PlayerStats _stats;

        public static PlayerController Instance { get; private set; }
        public Transform Transform => transform;
        public Vector2 MoveDirection => _moveDir;

        private Vector2 _moveDir;

        private float _animTimer;
        private int _animFrame;

        private bool _isDying;
        private float _deathAnimTimer;
        private int _deathAnimFrame;

        void Awake()
        {
            Instance = this;
            _stats = GetComponent<PlayerStats>();
        }

        void OnEnable()
        {
            _stats.OnDeath += HandleDeath;
        }

        void OnDisable()
        {
            _stats.OnDeath -= HandleDeath;
        }

        public void SetMoveDirection(Vector2 dir)
        {
            _moveDir = dir;
        }

        void Update()
        {
            if (_isDying)
            {
                _deathAnimTimer += Time.deltaTime;
                if (_deathAnimTimer >= 1f / deathFrameRate)
                {
                    _deathAnimTimer -= 1f / deathFrameRate;
                    _deathAnimFrame++;
                    if (_deathAnimFrame >= deathFrames.Length)
                    {
                        _isDying = false;
                        GameManager.Instance?.TriggerGameOver();
                        return;
                    }
                    if (spriteRenderer != null)
                        spriteRenderer.sprite = deathFrames[_deathAnimFrame];
                }
                return; 
            }

            if (GameManager.Instance?.State != GameState.Playing) 
                return;

            transform.position += (Vector3)(_moveDir * _stats.MoveSpeed * Time.deltaTime);

            if (_moveDir.x != 0f && spriteRenderer != null)
                spriteRenderer.flipX = _moveDir.x < 0f;

            // 걷기 애니메이션
            if (walkFrames != null && walkFrames.Length > 1 && spriteRenderer != null)
            {
                _animTimer += Time.deltaTime;
                if (_animTimer >= 1f / animFrameRate)
                {
                    _animTimer -= 1f / animFrameRate;
                    _animFrame = (_animFrame + 1) % walkFrames.Length;
                    spriteRenderer.sprite = walkFrames[_animFrame];
                }
            }
        }

        private void HandleDeath()
        {
            if (deathFrames != null && deathFrames.Length > 0)
            {
                _isDying = true;
                _deathAnimTimer = 0f;
                _deathAnimFrame = 0;
                if (spriteRenderer != null)
                    spriteRenderer.sprite = deathFrames[0];
            }
            else
            {
                GameManager.Instance?.TriggerGameOver();
            }
        }
    }
}
