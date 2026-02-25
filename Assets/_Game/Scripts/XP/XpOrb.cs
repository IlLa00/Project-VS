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

        private const float COLLECT_SQR_RADIUS = 0.5f * 0.5f;

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

            float sqrDist = ((Vector2)_player.position - (Vector2)transform.position).sqrMagnitude;

            // 플레이어가 직접 닿으면 XP 지급 후 풀 반환
            if (sqrDist <= COLLECT_SQR_RADIUS)
            {
                _playerXP?.AddXP(_xpAmount);
                SoundManager.Instance?.Play(SoundType.XpCollect);
                _returnToPool?.Invoke(this);
            }
        }
    }
}
