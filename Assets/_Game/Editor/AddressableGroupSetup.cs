using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace VS.Editor
{
    public static class AddressableGroupSetup
    {
        private static readonly Dictionary<string, string> PrefabAddressMap = new()
        {
            // Enemies
            { "Assets/_Game/Prefabs/Enemies/Enemy.prefab",          "Enemies/Enemy" },
            { "Assets/_Game/Prefabs/Enemies/EliteEnemy.prefab",     "Enemies/EliteEnemy" },
            { "Assets/_Game/Prefabs/Enemies/BossEnemy.prefab",      "Enemies/BossEnemy" },
            { "Assets/_Game/Prefabs/Enemies/EnemyProjectile.prefab","Enemies/EnemyProjectile" },

            // Weapons
            { "Assets/_Game/Prefabs/Weapons/ProjectileWeapon.prefab","Weapons/ProjectileWeapon" },
            { "Assets/_Game/Prefabs/Weapons/Projectile.prefab",      "Weapons/Projectile" },
            { "Assets/_Game/Prefabs/Weapons/OrbitalWeapon.prefab",   "Weapons/OrbitalWeapon" },
            { "Assets/_Game/Prefabs/Weapons/LightningWeapon.prefab", "Weapons/LightningWeapon" },
            { "Assets/_Game/Prefabs/Weapons/LightningStrike.prefab", "Weapons/LightningStrike" },
            { "Assets/_Game/Prefabs/Weapons/BeamWeapon.prefab",      "Weapons/BeamWeapon" },

            // UI
            { "Assets/_Game/Prefabs/UI/DamageFloater.prefab", "UI/DamageFloater" },

            // Gameplay
            { "Assets/_Game/Prefabs/Player/Player.prefab", "Player/Player" },
            { "Assets/_Game/Prefabs/Xp/XpOrb.prefab",     "XP/XpOrb" },
        };

        private static readonly Dictionary<string, string> AddressToGroup = new()
        {
            { "Enemies",  "Enemies" },
            { "Weapons",  "Weapons" },
            { "UI",       "UI" },
            { "Player",   "Gameplay" },
            { "XP",       "Gameplay" },
        };

        [MenuItem("VS/Setup Addressable Groups")]
        public static void Setup()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("Addressable Settings가 없습니다. Window > Asset Management > Addressables > Groups에서 먼저 초기화해주세요.");
                return;
            }

            foreach (var kv in PrefabAddressMap)
            {
                string assetPath = kv.Key;
                string address   = kv.Value;

                string guid = AssetDatabase.AssetPathToGUID(assetPath);
                if (string.IsNullOrEmpty(guid))
                {
                    Debug.LogWarning($"에셋을 찾을 수 없음: {assetPath}");
                    continue;
                }

                string category  = address.Split('/')[0];
                string groupName = AddressToGroup[category];
                var group        = GetOrCreateGroup(settings, groupName);

                var entry = settings.CreateOrMoveEntry(guid, group);
                entry.address = address;

                Debug.Log($"[Addressables] {address} → 그룹 '{groupName}'");
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Addressable 그룹 설정 완료");
        }

        private static AddressableAssetGroup GetOrCreateGroup(AddressableAssetSettings settings, string groupName)
        {
            var group = settings.FindGroup(groupName);
            if (group != null)
                return group;

            group = settings.CreateGroup(groupName, false, false, false, null,
                typeof(ContentUpdateGroupSchema),
                typeof(BundledAssetGroupSchema));

            var schema = group.GetSchema<BundledAssetGroupSchema>();
            schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;

            return group;
        }
    }
}
