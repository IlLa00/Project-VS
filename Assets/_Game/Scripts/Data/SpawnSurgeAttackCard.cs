using System.Collections.Generic;
using UnityEngine;

namespace VS.Data
{
    [CreateAssetMenu(menuName = "AttackCards/SpawnSurge")]
    public class SpawnSurgeAttackCard : AttackCardDataBase
    {
        public float multiplier = 3f;
        public float duration = 10f;

        public override string GetAttackType() => "spawn_surge";

        public override Dictionary<string, object> GetParameters() => new Dictionary<string, object>
        {
            { "multiplier", multiplier },
            { "duration", duration }
        };
    }
}
