using UnityEngine;
using VS.Player;
using VS.Weapons;

namespace VS.Data
{
    /// <summary>
    /// 무기 스탯에 해당하는 항목들.
    /// 각 무기는 ApplyUpgrade()에서 자신이 지원하는 타입만 처리하고 나머지는 무시한다.
    /// </summary>
    public enum WeaponStatType
    {
        DamageUp,          // 데미지 증가         (투사체 / 회전 무기 공통)
        FireRateUp,        // 연사속도 증가        (투사체 무기)
        PierceUp,          // 관통 횟수 증가       (투사체 무기)
        RotationSpeedUp,   // 회전 속도 증가       (회전 무기)
        OrbRadiusUp,       // 궤도 반경 증가       (회전 무기)
    }

    /// <summary>
    /// 지정한 무기의 특정 스탯을 강화하는 카드.
    /// weaponPrefab으로 어떤 무기를 강화할지 지정한다.
    /// 플레이어가 해당 무기를 보유하지 않은 경우 효과가 없다.
    /// </summary>
    [CreateAssetMenu(fileName = "NewWeaponStatUpgrade", menuName = "VS/Upgrades/무기 스탯")]
    public class WeaponStatUpgradeData : UpgradeDataBase
    {
        [Tooltip("강화할 무기의 프리팹 (타입 식별용)")]
        public GameObject weaponPrefab;
        public WeaponStatType statType;
        public float value;

        public override void Apply(PlayerController player)
        {
            if (weaponPrefab == null) return;

            // 프리팹의 IUpgradableWeapon 컴포넌트 타입으로 플레이어에서 해당 무기를 찾음
            var prefabWeapon = weaponPrefab.GetComponent<IUpgradableWeapon>();
            if (prefabWeapon == null) return;

            var weaponType = ((MonoBehaviour)prefabWeapon).GetType();
            var existing = player.Transform.GetComponentInChildren(weaponType) as IUpgradableWeapon;
            existing?.ApplyUpgrade(statType, value);
        }
    }
}
