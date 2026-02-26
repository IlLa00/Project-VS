using System;
using System.Collections.Generic;
using UnityEngine;
using VS.Core;
using VS.Enemies;

namespace VS.Weapons
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class Projectile : MonoBehaviour
    {
        private Vector2 _direction;
        private float _speed;
        private float _damage;
        private float _maxRange;
        private int _remainPierce;
        private Vector2 _startPos;
        private Action<Projectile> _returnToPool;

        private readonly HashSet<EnemyBase> _hitEnemies = new HashSet<EnemyBase>();
        private bool _isDead;
        private Rigidbody2D _rb;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.gravityScale = 0f;

            var col = GetComponent<CircleCollider2D>();
            col.isTrigger = true;
        }

        public void Init(Vector2 direction, float speed, float damage, float maxRange, int pierceCount, Action<Projectile> returnToPool)
        {
            _direction = direction;
            _speed = speed;
            _damage = damage;
            _maxRange = maxRange;
            _remainPierce = pierceCount;
            _returnToPool = returnToPool;
            _startPos = _rb.position;
            _isDead = false;
            _hitEnemies.Clear();
        }

        void FixedUpdate()
        {
            if (_isDead) return;

            Vector2 newPos = _rb.position + _direction * _speed * Time.fixedDeltaTime;
            _rb.MovePosition(newPos);

            if (Vector2.SqrMagnitude(newPos - _startPos) >= _maxRange * _maxRange)
                ReturnSelf();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (_isDead) return;

            EnemyBase enemy = other.GetComponent<EnemyBase>();
            if (enemy == null) return;
            if (_hitEnemies.Contains(enemy)) return;

            enemy.TakeDamage(_damage);
            _hitEnemies.Add(enemy);

            if (_remainPierce <= 0) 
            {
                ReturnSelf();
                return;
            }
            _remainPierce--;
        }

        private void ReturnSelf()
        {
            _isDead = true;
            _returnToPool?.Invoke(this);
        }
    }
}
