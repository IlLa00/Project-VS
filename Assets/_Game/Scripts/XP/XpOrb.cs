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

        private bool _isAttracted;

        private const float COLLECT_SQR_RADIUS = 0.5f * 0.5f;
        private const float ATTRACT_SPEED = 12f;

        void OnEnable()
        {
            _isAttracted = false;

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

        public void Attract()
        {
            _isAttracted = true;
        }

        void Update()
        {
            if (GameManager.Instance?.State != GameState.Playing)
                return;

            if (_player == null)
                return;

            if (_isAttracted)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    _player.position,
                    ATTRACT_SPEED * Time.deltaTime);
            }

            float sqrDist = ((Vector2)_player.position - (Vector2)transform.position).sqrMagnitude;
            if (sqrDist <= COLLECT_SQR_RADIUS)
            {
                _playerXP?.AddXP(_xpAmount);
                SoundManager.Instance?.Play(SoundType.XpCollect);
                _returnToPool?.Invoke(this);
            }
        }
    }
}
