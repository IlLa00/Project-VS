using UnityEngine;
using VS.Player;
using VS.Weapons;

namespace VS.Data
{
    [CreateAssetMenu(fileName = "NewProjectileUpgrade", menuName = "VS/Upgrades/투사체 무기 강화")]
    public class ProjectileWeaponUpgradeData : UpgradeDataBase
    {
        public override void Apply(PlayerController player)
        {
            player.GetComponentInChildren<ProjectileWeapon>()?.AddProjectile();
        }

        public override bool IsApplicable(PlayerController player)
        {
            var weapon = player.GetComponentInChildren<ProjectileWeapon>();
            return weapon != null && weapon.CanUpgrade;
        }
    }
}
