using UnityEngine;
using VS.Core;

namespace VS.Player
{
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        private PlayerStats _stats;

        public static PlayerController Instance { get; private set; }
        public Transform Transform => transform;

        private Vector2 _moveDir;

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
            if (GameManager.Instance?.State != GameState.Playing) return;

            transform.position += (Vector3)(_moveDir * _stats.MoveSpeed * Time.deltaTime);

            if (_moveDir.x != 0f && spriteRenderer != null)
                spriteRenderer.flipX = _moveDir.x < 0f;
        }

        private void HandleDeath()
        {
            GameManager.Instance?.TriggerGameOver();
        }
    }
}
