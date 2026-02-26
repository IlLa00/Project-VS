using System.Collections.Generic;
using UnityEngine;
using VS.Data;

namespace VS.Weapons
{
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] private ProjectileWeapon weaponPrefab;
        [SerializeField] private WeaponData startWeapon;

        private readonly List<ProjectileWeapon> _weapons = new List<ProjectileWeapon>();

        void Start()
        {
            if (startWeapon != null)
                AddWeapon(startWeapon);
        }

        public ProjectileWeapon AddWeapon(WeaponData data)
        {
            ProjectileWeapon weapon = Instantiate(weaponPrefab, transform);
            weapon.Init(data);
            _weapons.Add(weapon);
            return weapon;
        }

        public IReadOnlyList<ProjectileWeapon> Weapons => _weapons;
    }
}
