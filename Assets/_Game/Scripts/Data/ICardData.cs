using UnityEngine;

namespace VS.Data
{
    public interface ICardData
    {
        string CardName { get; }
        string Description { get; }
        Sprite Icon { get; }
    }
}
