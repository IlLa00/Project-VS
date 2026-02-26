using UnityEngine;
using VS.Player;
using VS.Weapons;

namespace VS.Data
{
    [CreateAssetMenu(fileName = "NewOrbitalUpgrade", menuName = "VS/Upgrades/오비탈 무기 강화")]
    public class OrbitalWeaponUpgradeData : UpgradeDataBase
    {
        public override void Apply(PlayerController player)
        {
            player.GetComponentInChildren<OrbitalWeapon>()?.Upgrade();
        }

        public override bool IsApplicable(PlayerController player)
        {
            var weapon = player.GetComponentInChildren<OrbitalWeapon>();
            return weapon != null && weapon.CanUpgrade;
        }
    }
}
