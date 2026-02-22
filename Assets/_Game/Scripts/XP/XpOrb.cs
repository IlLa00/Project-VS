using System;
using UnityEngine;
using VS.Core;
using VS.Player;

namespace VS.XP
{
    public class XpOrb : MonoBehaviour
    {
        private float _xpAmount;
        private Action<XpOrb> _returnToPool;

        private Transform _player;
        private PlayerXP _playerXP;

        private const float ATTRACT_RADIUS = 3f;
        private const float ATTRACT_SPEED = 10f;
        private const float COLLECT_SQR_RADIUS = 0.3f * 0.3f;

        void OnEnable()
        {
            var pc = PlayerController.Instance;
            if (pc != null)
            {
                _player = pc.Transform;
                _playerXP = pc.GetComponent<PlayerXP>();
            }
        }

        public void Init(float xpAmount, Action<XpOrb> returnToPool)
        {
            _xpAmount = xpAmount;
            _returnToPool = returnToPool;
        }

        void Update()
        {
            if (GameManager.Instance?.State != GameState.Playing) return;
            if (_player == null) return;

            Vector2 toPlayer = (Vector2)_player.position - (Vector2)transform.position;
            float sqrDist = toPlayer.sqrMagnitude;

            // 흡수 범위에 도달하면 XP 지급 후 풀 반환
            if (sqrDist <= COLLECT_SQR_RADIUS)
            {
                _playerXP?.AddXP(_xpAmount);
                _returnToPool?.Invoke(this);
                return;
            }

            // 인력 범위 안에 있으면 플레이어 방향으로 이동
            if (sqrDist <= ATTRACT_RADIUS * ATTRACT_RADIUS)
            {
                transform.position += (Vector3)(toPlayer.normalized * ATTRACT_SPEED * Time.deltaTime);
            }
        }
    }
}
