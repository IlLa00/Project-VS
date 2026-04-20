using System.Collections.Generic;
using UnityEngine;
using VS.Core;

namespace VS.XP
{
    public class XpOrbSpawner : PickupSpawnerBase<XpOrb>
    {
        public static XpOrbSpawner Instance { get; private set; }

        private readonly HashSet<XpOrb> _activeOrbs = new HashSet<XpOrb>();

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
        }

        public void Spawn(Vector2 position, float xpAmount)
        {
            XpOrb orb = Pool.Get();
            orb.transform.position = position;
            orb.Init(xpAmount, ReturnOrb);
            _activeOrbs.Add(orb);
        }

        private void ReturnOrb(XpOrb orb)
        {
            _activeOrbs.Remove(orb);
            ReturnToPool(orb);
        }

        public IReadOnlyCollection<XpOrb> GetActiveOrbs() => _activeOrbs;
    }
}
