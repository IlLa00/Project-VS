using UnityEngine;
using VS.Player;
using VS.Weapons;

namespace VS.Data
{
    [CreateAssetMenu(fileName = "NewLightningUpgrade", menuName = "VS/Upgrades/번개 무기 강화")]
    public class LightningWeaponUpgradeData : UpgradeDataBase
    {
        public override void Apply(PlayerController player)
        {
            player.GetComponentInChildren<LightningWeapon>()?.Upgrade();
        }

        public override bool IsApplicable(PlayerController player)
        {
            var weapon = player.GetComponentInChildren<LightningWeapon>();
            return weapon != null && weapon.CanUpgrade;
        }
    }
}
