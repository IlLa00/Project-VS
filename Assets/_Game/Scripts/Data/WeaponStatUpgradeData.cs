using UnityEngine;
using VS.Player;
using VS.Weapons;

namespace VS.Data
{
    public enum WeaponStatType
    {
        DamageUp,          // 데미지 증가         (투사체 / 회전 무기 공통)
        FireRateUp,        // 연사속도 증가        (투사체 무기)
        PierceUp,          // 관통 횟수 증가       (투사체 무기)
        RotationSpeedUp,   // 회전 속도 증가       (회전 무기)
        OrbRadiusUp,       // 궤도 반경 증가       (회전 무기)
    }

    [CreateAssetMenu(fileName = "NewWeaponStatUpgrade", menuName = "VS/Upgrades/무기 스탯")]
    public class WeaponStatUpgradeData : UpgradeDataBase
    {
        [Tooltip("강화할 무기의 프리팹 (타입 식별용)")]
        public GameObject weaponPrefab;
        public WeaponStatType statType;
        public float value;

        public override void Apply(PlayerController player)
        {
            if (weaponPrefab == null) 
                return;

            var prefabWeapon = weaponPrefab.GetComponent<IUpgradableWeapon>();
            if (prefabWeapon == null) 
                return;

            var weaponType = ((MonoBehaviour)prefabWeapon).GetType();
            var existing = player.Transform.GetComponentInChildren(weaponType) as IUpgradableWeapon;
            existing?.ApplyUpgrade(statType, value);
        }

        public override bool IsApplicable(PlayerController player)
        {
            if (weaponPrefab == null) 
                return false;

            var prefabWeapon = weaponPrefab.GetComponent<IUpgradableWeapon>();
            if (prefabWeapon == null) 
                return false;

            var weaponType = ((MonoBehaviour)prefabWeapon).GetType();
            var existing = player.Transform.GetComponentInChildren(weaponType) as IUpgradableWeapon;

            // 무기 미보유: 카드 숨김 / 보유 + 업그레이드 가능: 표시 / 만렙: 숨김
            return existing != null && existing.CanUpgrade;
        }
    }
}
