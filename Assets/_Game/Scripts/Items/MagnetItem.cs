using System;
using UnityEngine;
using VS.Core;
using VS.Player;
using VS.XP;

namespace VS.Items
{
    public class MagnetItem : MonoBehaviour
    {
        private Action<MagnetItem> _returnToPool;
        private Transform _player;

        private const float COLLECT_SQR_RADIUS = 0.5f * 0.5f;

        void OnEnable()
        {
            var pc = PlayerController.Instance;
            if (pc != null)
                _player = pc.Transform;
        }

        public void Init(Action<MagnetItem> returnToPool)
        {
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
                AttractAllOrbs();
                _returnToPool?.Invoke(this);
            }
        }

        private void AttractAllOrbs()
        {
            if (XpOrbSpawner.Instance == null)
                return;

            foreach (var orb in XpOrbSpawner.Instance.GetActiveOrbs())
                orb.Attract();
        }
    }
}
