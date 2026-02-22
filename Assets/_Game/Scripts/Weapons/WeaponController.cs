using System.Collections.Generic;
using UnityEngine;
using VS.Data;

namespace VS.Weapons
{
    /// <summary>
    /// Player에 붙이는 무기 관리 컴포넌트.
    /// 레벨업 시스템에서 AddWeapon / UpgradeWeapon을 호출해 무기를 추가·강화한다.
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] private WeaponBase weaponPrefab;
        [SerializeField] private WeaponData startWeapon;

        private readonly List<WeaponBase> _weapons = new List<WeaponBase>();

        void Start()
        {
            if (startWeapon != null)
                AddWeapon(startWeapon);
        }

        /// <summary>새 무기를 추가한다.</summary>
        public WeaponBase AddWeapon(WeaponData data)
        {
            WeaponBase weapon = Instantiate(weaponPrefab, transform);
            weapon.Init(data);
            _weapons.Add(weapon);
            return weapon;
        }

        public IReadOnlyList<WeaponBase> Weapons => _weapons;
    }
}
