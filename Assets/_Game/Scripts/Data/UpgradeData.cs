using UnityEngine;
using VS.Player;

namespace VS.Data
{
    /// <summary>
    /// 레벨업 카드 공통 베이스.
    /// 새 업그레이드를 추가할 때는 이 클래스를 상속해 Apply()만 구현한다.
    /// 에셋은 Assets/_Game/Resources/Upgrades/ 에 두면 자동으로 레벨업 풀에 포함된다.
    /// </summary>
    public abstract class UpgradeDataBase : ScriptableObject
    {
        public string upgradeName;
        [TextArea] public string description;
        public Sprite icon;

        /// <summary>카드 선택 시 호출. 플레이어에게 효과를 적용한다.</summary>
        public abstract void Apply(PlayerController player);

        /// <summary>이 카드가 현재 선택지에 표시될 수 있는지 여부. 만렙 무기 카드 등을 걸러낸다.</summary>
        public virtual bool IsApplicable(PlayerController player) => true;
    }
}
