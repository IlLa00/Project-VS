using System.Collections.Generic;
using UnityEngine;

namespace VS.Data
{
    [CreateAssetMenu(menuName = "AttackCards/WeaponDowngrade")]
    public class WeaponDowngradeAttackCard : AttackCardDataBase
    {
        public override string GetAttackType() => "weapon_downgrade";

        public override Dictionary<string, object> GetParameters() => new Dictionary<string, object>();
    }
}
