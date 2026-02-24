using UnityEngine;
using VS.Player;
using VS.Weapons;

namespace VS.Data
{
    /// <summary>
    /// 무기 획득 카드. weaponPrefab을 플레이어 자식으로 생성한다.
    /// 프리팹이 IStackableWeapon을 구현하면 중복 선택 시 AddStack()을 호출한다.
    /// 새 무기를 추가할 때는 이 에셋을 하나 더 만들고 weaponPrefab만 교체하면 된다.
    /// </summary>
    [CreateAssetMenu(fileName = "NewWeaponAcquire", menuName = "VS/Upgrades/무기 획득")]
    public class WeaponAcquireData : UpgradeDataBase
    {
        [Tooltip("플레이어 자식으로 생성할 무기 프리팹")]
        public GameObject weaponPrefab;

        public override void Apply(PlayerController player)
        {
            if (weaponPrefab == null) return;

            // 프리팹에 IStackableWeapon이 있으면 이미 보유 중인지 확인 후 스택
            var prefabStackable = weaponPrefab.GetComponent<IStackableWeapon>();
            if (prefabStackable != null)
            {
                var existingType = ((MonoBehaviour)prefabStackable).GetType();
                var existing = player.Transform.GetComponentInChildren(existingType) as IStackableWeapon;
                if (existing != null)
                {
                    existing.AddStack();
                    return;
                }
            }

            Object.Instantiate(weaponPrefab, player.Transform);
        }
    }
}
