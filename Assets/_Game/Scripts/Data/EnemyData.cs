using UnityEngine;

namespace VS.Data
{
    public enum EnemyType { Normal, Elite, Boss }

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

        [Header("걷기 애니메이션")]
        public Sprite[] walkFrames;
        [Range(1f, 24f)]
        public float animFrameRate = 8f;

        [Header("사망 애니메이션")]
        public Sprite[] deathFrames;
        [Range(1f, 24f)]
        public float deathFrameRate = 8f;

        [Header("엘리트 전용 — 사망 시 플레이어 버프")]
        public BuffStatType buffStatType;
        public float buffValue;    
        public float buffDuration; 
    }
}
