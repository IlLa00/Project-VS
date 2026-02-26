using UnityEngine;
using VS.Player;
using VS.Weapons;

namespace VS.Data
{
    [CreateAssetMenu(fileName = "NewBeamUpgrade", menuName = "VS/Upgrades/빔 무기 강화")]
    public class BeamWeaponUpgradeData : UpgradeDataBase
    {
        public override void Apply(PlayerController player)
        {
            player.GetComponentInChildren<BeamWeapon>()?.Upgrade();
        }

        public override bool IsApplicable(PlayerController player)
        {
            var weapon = player.GetComponentInChildren<BeamWeapon>();
            return weapon != null && weapon.CanUpgrade;
        }
    }
}
