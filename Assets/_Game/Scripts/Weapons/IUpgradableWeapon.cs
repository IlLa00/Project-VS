namespace VS.Weapons
{
    public interface IUpgradableWeapon
    {
        int UpgradeLevel { get; }
        bool CanUpgrade { get; }
    }
}
