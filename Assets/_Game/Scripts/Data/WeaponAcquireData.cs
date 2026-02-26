using UnityEngine;
using VS.Player;
using VS.Weapons;

namespace VS.Data
{
    [CreateAssetMenu(fileName = "NewWeaponAcquire", menuName = "VS/Upgrades/무기 획득")]
    public class WeaponAcquireData : UpgradeDataBase
    {
        [Tooltip("플레이어 자식으로 생성할 무기 프리팹")]
        public GameObject weaponPrefab;

        public override void Apply(PlayerController player)
        {
            if (weaponPrefab == null)
                return;

            if (!IsApplicable(player))
                return;

            var inventory = player.GetComponent<WeaponInventory>();
            if (inventory != null)
                inventory.AddWeapon(this);
            else
                Object.Instantiate(weaponPrefab, player.Transform);
        }

        public override bool IsApplicable(PlayerController player)
        {
            if (weaponPrefab == null) return false;

            var prefabUpgradable = weaponPrefab.GetComponent<IUpgradableWeapon>();
            if (prefabUpgradable == null) 
                return true; 

            var weaponType = ((MonoBehaviour)prefabUpgradable).GetType();
            
            return player.Transform.GetComponentInChildren(weaponType) == null;
        }
    }
}
