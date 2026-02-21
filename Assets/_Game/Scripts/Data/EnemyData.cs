using UnityEngine;

namespace VS.Data
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "VS/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        public string enemyName = "Enemy";
        public float maxHp = 10f;
        public float moveSpeed = 2f;
        public float contactDamage = 10f;
        public float xpDrop = 5f;
        public Sprite sprite;
        public Color color = Color.white;
    }
}
