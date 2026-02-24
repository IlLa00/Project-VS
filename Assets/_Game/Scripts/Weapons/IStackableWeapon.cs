namespace VS.Weapons
{
    /// <summary>
    /// 레벨업 카드에서 중복 획득 시 스택(강화)이 가능한 무기 인터페이스.
    /// 구현 클래스는 AddStack()에서 오브 추가, 레벨 강화 등 원하는 로직을 넣는다.
    /// </summary>
    public interface IStackableWeapon
    {
        void AddStack();
    }
}
