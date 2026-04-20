using VS.Data;

namespace VS.Core
{
    public static class WeaponSelectManager
    {
        public static WeaponAcquireData SelectedWeapon { get; private set; }

        public static void Select(WeaponAcquireData data)
        {
            SelectedWeapon = data;
        }

        public static void Clear()
        {
            SelectedWeapon = null;
        }
    }
}