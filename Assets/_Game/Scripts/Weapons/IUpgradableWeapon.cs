using VS.Data;

namespace VS.Weapons
{
    /// <summary>
    /// 레벨업 카드로 특정 무기의 스탯을 강화할 수 있는 인터페이스.
    /// 새 무기 추가 시 이 인터페이스를 구현하고 지원할 WeaponStatType 케이스를 처리한다.
    /// 지원하지 않는 stat 타입은 단순히 무시하면 된다.
    /// </summary>
    public interface IUpgradableWeapon
    {
        int UpgradeLevel { get; }
        bool CanUpgrade { get; }   // UpgradeLevel < MAX_UPGRADE (5)
        void ApplyUpgrade(WeaponStatType stat, float value);
    }
}
