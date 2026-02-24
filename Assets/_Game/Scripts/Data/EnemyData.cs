using UnityEngine;

namespace VS.Data
{
    public enum EnemyType { Normal, Elite, Boss }

    /// <summary>엘리트 사망 버프에 사용하는 스탯 종류</summary>
    public enum BuffStatType { DamageUp, SpeedUp, FireRateUp }

    [CreateAssetMenu(fileName = "EnemyData", menuName = "VS/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [Header("공통 스탯")]
        public string enemyName = "Enemy";
        public EnemyType enemyType = EnemyType.Normal;
        public float maxHp = 10f;
        public float moveSpeed = 2f;
        public float contactDamage = 10f;
        public float xpDrop = 5f;
        public Sprite sprite;
        public Color color = Color.white;

        [Header("엘리트 전용 — 사망 시 플레이어 버프")]
        public BuffStatType buffStatType;
        public float buffValue;    // 버프 수치 (예: 이동속도 +2)
        public float buffDuration; // 지속 시간 (초)
    }
}
