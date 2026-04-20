using System;
using UnityEngine;
using VS.Core;
using VS.Enemies;
using VS.Player;

namespace VS.Items
{
    public class BombItem : MonoBehaviour
    {
        private float _damage;
        private Action<BombItem> _returnToPool;
        private Transform _player;

        private const float COLLECT_SQR_RADIUS = 0.5f * 0.5f;

        void OnEnable()
        {
            var pc = PlayerController.Instance;
            if (pc != null)
                _player = pc.Transform;
        }

        public void Init(float damage, Action<BombItem> returnToPool)
        {
            _damage = damage;
            _returnToPool = returnToPool;
        }

        void Update()
        {
            if (GameManager.Instance?.State != GameState.Playing)
                return;

            if (_player == null)
                return;

            float sqrDist = ((Vector2)_player.position - (Vector2)transform.position).sqrMagnitude;
            if (sqrDist <= COLLECT_SQR_RADIUS)
            {
                Explode();
                _returnToPool?.Invoke(this);
            }
        }

        private void Explode()
        {
            var targets = EnemyBase.ActiveEnemies.ToArray();
            Camera cam = Camera.main;

            foreach (var enemy in targets)
            {
                if (enemy == null) continue;

                Vector3 vp = cam.WorldToViewportPoint(enemy.transform.position);
                if (vp.x >= 0f && vp.x <= 1f && vp.y >= 0f && vp.y <= 1f)
                    enemy.TakeDamage(_damage);
            }

            SoundManager.Instance?.Play(SoundType.EnemyHit);
        }
    }
}
