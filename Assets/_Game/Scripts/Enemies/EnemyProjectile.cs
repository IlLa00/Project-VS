using UnityEngine;
using VS.Player;

namespace VS.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class EnemyProjectile : MonoBehaviour
    {
        private Vector2 _direction;
        private float _speed;
        private float _damage;
        private float _maxRange;
        private Vector2 _startPos;
        private Rigidbody2D _rb;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.gravityScale = 0f;

            var col = GetComponent<CircleCollider2D>();
            col.isTrigger = true;
        }

        public void Init(Vector2 direction, float speed, float damage, float maxRange)
        {
            _direction = direction.normalized;
            _speed = speed;
            _damage = damage;
            _maxRange = maxRange;
            _startPos = _rb.position;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        void FixedUpdate()
        {
            Vector2 newPos = _rb.position + _direction * _speed * Time.fixedDeltaTime;
            _rb.MovePosition(newPos);

            if (Vector2.SqrMagnitude(newPos - _startPos) >= _maxRange * _maxRange)
                Destroy(gameObject);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            var stats = other.GetComponent<PlayerStats>();
            if (stats == null) return;

            stats.TakeDamage(_damage);
            Destroy(gameObject);
        }
    }
}
