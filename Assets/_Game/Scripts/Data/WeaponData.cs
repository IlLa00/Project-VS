using UnityEngine;

namespace VS.Data
{
    [CreateAssetMenu(fileName = "WeaponData", menuName = "VS/WeaponData")]
    public class WeaponData : ScriptableObject
    {
        [Header("기본 정보")]
        public string weaponName = "기본 무기";

        [Header("공격 스탯")]
        public float damage = 10f;
        public float fireRate = 1f;         // 초당 발사 횟수

        [Header("투사체 설정")]
        public float projectileSpeed = 8f;
        public float projectileRange = 10f; // 최대 사거리
        public int pierceCount = 0;         // 관통 횟수 (0 = 1회 히트)
        public float projectileScale = 0.3f;
    }
}
