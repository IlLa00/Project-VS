using UnityEngine;
using VS.Player;
using VS.Weapons;

namespace VS.Data
{
    /// <summary>
    /// 무기 획득 카드. weaponPrefab을 플레이어 자식으로 생성한다.
    /// 무기는 1개씩만 보유 가능하며, 이미 보유 중이면 카드가 표시되지 않는다.
    /// 새 무기 추가 시 이 에셋을 하나 더 만들고 weaponPrefab만 교체하면 된다.
    /// </summary>
    [CreateAssetMenu(fileName = "NewWeaponAcquire", menuName = "VS/Upgrades/무기 획득")]
    public class WeaponAcquireData : UpgradeDataBase
    {
        [Tooltip("플레이어 자식으로 생성할 무기 프리팹")]
        public GameObject weaponPrefab;

        public override void Apply(PlayerController player)
        {
            if (weaponPrefab == null) return;
            if (!IsApplicable(player)) return; // 이미 보유 중이면 무시

            Object.Instantiate(weaponPrefab, player.Transform);
        }

        public override bool IsApplicable(PlayerController player)
        {
            if (weaponPrefab == null) return false;

            // IUpgradableWeapon 타입으로 이미 보유 중인지 확인
            var prefabUpgradable = weaponPrefab.GetComponent<IUpgradableWeapon>();
            if (prefabUpgradable == null) return true; // 업그레이드 불가 무기는 항상 획득 가능

            var weaponType = ((MonoBehaviour)prefabUpgradable).GetType();
            return player.Transform.GetComponentInChildren(weaponType) == null;
        }
    }
}
