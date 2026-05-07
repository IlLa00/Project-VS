using System.Collections.Generic;
using UnityEngine;

namespace VS.Data
{
    public abstract class AttackCardDataBase : ScriptableObject, ICardData
    {
        public string cardName;
        [TextArea] public string description;
        public Sprite icon;

        public string CardName => cardName;
        public string Description => description;
        public Sprite Icon => icon;

        public abstract string GetAttackType();
        public abstract Dictionary<string, object> GetParameters();
    }
}
