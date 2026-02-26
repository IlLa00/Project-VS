using UnityEngine;
using VS.Player;

namespace VS.Data
{
    public enum CharacterStatType { DamageUp, SpeedUp, MaxHpUp, HpRestore }

    [CreateAssetMenu(fileName = "NewCharacterUpgrade", menuName = "VS/Upgrades/캐릭터 스탯")]
    public class CharacterUpgradeData : UpgradeDataBase
    {
        public CharacterStatType statType;
        public float value;

        public override void Apply(PlayerController player)
        {
            var stats = player.GetComponent<PlayerStats>();
            if (stats == null) return;

            switch (statType)
            {
                case CharacterStatType.DamageUp:  stats.AddDamageMultiplier(value); break;
                case CharacterStatType.SpeedUp:   stats.AddMoveSpeed(value);        break;
                case CharacterStatType.MaxHpUp:   stats.AddMaxHp(value);            break;
                case CharacterStatType.HpRestore: stats.Heal(value);                break;
            }
        }
    }
}
