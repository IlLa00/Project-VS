using UnityEngine;

namespace VS.Data
{
    public enum UpgradeType { DamageUp, SpeedUp, MaxHpUp, HpRestore }

    [CreateAssetMenu(fileName = "UpgradeData", menuName = "VS/UpgradeData")]
    public class UpgradeData : ScriptableObject
    {
        public string upgradeName;
        [TextArea] public string description;
        public UpgradeType upgradeType;
        public float value;
        public Sprite icon;
    }
}
