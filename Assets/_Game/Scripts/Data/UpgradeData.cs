using UnityEngine;
using VS.Player;

namespace VS.Data
{
    public abstract class UpgradeDataBase : ScriptableObject
    {
        public string upgradeName;
        [TextArea] public string description;
        public Sprite icon;

        public abstract void Apply(PlayerController player);
        public virtual bool IsApplicable(PlayerController player) => true;
    }
}
