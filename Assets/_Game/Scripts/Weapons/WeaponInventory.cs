using System;
using System.Collections.Generic;
using UnityEngine;
using VS.Data;

namespace VS.Weapons
{
    public class WeaponInventory : MonoBehaviour
    {
        [SerializeField] private WeaponAcquireData[] startWeapons;

        public struct WeaponEntry
        {
            public IUpgradableWeapon Weapon;
            public WeaponAcquireData Data;
        }

        private readonly List<WeaponEntry> _weapons = new List<WeaponEntry>();
        public IReadOnlyList<WeaponEntry> Weapons => _weapons;

        public event Action OnWeaponsChanged;

        void Start()
        {
            foreach (var data in startWeapons)
                AddWeapon(data);
        }

        public void AddWeapon(WeaponAcquireData data)
        {
            if (data?.weaponPrefab == null) return;

            var go = Instantiate(data.weaponPrefab, transform);
            var weapon = go.GetComponent<IUpgradableWeapon>();

            _weapons.Add(new WeaponEntry { Weapon = weapon, Data = data });
            OnWeaponsChanged?.Invoke();
        }
    }
}
