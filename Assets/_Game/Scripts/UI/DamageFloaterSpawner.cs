using UnityEngine;

namespace VS.UI
{
    public class DamageFloaterSpawner : MonoBehaviour
    {
        public static DamageFloaterSpawner Instance { get; private set; }

        [SerializeField] private DamageFloater floaterPrefab;
        [SerializeField] private float spawnHeightOffset = 0.5f;

        void Awake()
        {
            if (Instance != null) 
            { 
                Destroy(gameObject); 
                return; 
            }
            Instance = this;
        }

        public void Show(float damage, bool isKill, Vector3 worldPos)
        {
            if (floaterPrefab == null) 
                return;


            DamageFloater floater = Instantiate(
                floaterPrefab,
                worldPos + Vector3.up * spawnHeightOffset,
                Quaternion.identity);
                
            floater.Init(damage, isKill);
        }
    }
}
