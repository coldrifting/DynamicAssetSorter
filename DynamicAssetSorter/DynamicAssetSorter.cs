using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace DynamicAssetSorter
{
    public class DynamicAssetSorter
    {
        internal static bool patched;
        internal const string HarmonyId = "coldrifting.DynamicAssetSorter";

        internal static void PatchAll()
        {
            if (patched)
                return;

            patched = true;

            var harmony = new Harmony(HarmonyId);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        internal static void UnpatchAll()
        {
            if (!patched)
                return;

            var harmony = new Harmony(HarmonyId);
            harmony.UnpatchAll(HarmonyId);
            patched = false;
        }

        public static void Update()
        {
            ApplyRules();
            RefreshUI();
        }

        public static void ApplyRules()
        {
            foreach (PrefabRule rule in ModSettings.prefabRules)
            {
                // Find the right prefab collection
                PrefabInfo prefab;
                string prefabString = rule.PrefabType;
                switch (prefabString)
                {
                    case "Network":
                        prefab = PrefabCollection<NetInfo>.FindLoaded(rule.PrefabName);
                        break;
                    case "Building":
                        prefab = PrefabCollection<BuildingInfo>.FindLoaded(rule.PrefabName);
                        break;
                    case "Transport":
                        prefab = PrefabCollection<TransportInfo>.FindLoaded(rule.PrefabName);
                        break;
                    default:
                        Debug.Log($"Dynamic Asset Sorter - Could not convert prefab type {rule.PrefabType} for Prefab {rule.PrefabName}!");
                        continue;
                }

                // Set the priority
                if (prefab != null)
                    prefab.m_UIPriority = rule.Priority;
            }
        }

        // Forces a UIPanel Refresh. Useful to reload prefab icons.
        public static void RefreshUI()
        {
            MainToolbar mainToolbar = GameObject.FindObjectOfType<MainToolbar>();

            if (mainToolbar != null)
                mainToolbar.RefreshPanel();
        }

        public static bool isInGame()
        {
            if (LoadingManager.instance.m_loadedEnvironment != null)
                return true;

            return false;
        }

        // Make vanilla prefabs sort next to custom ones.
        // Used for most panels
        [HarmonyPatch(typeof(GeneratedScrollPanel))]
        [HarmonyPatch("ItemsGenericSort")]
        [HarmonyPatch(new System.Type[] { typeof(PrefabInfo), typeof(PrefabInfo) })]
        public static class GeneratedScrollPanelItemsGenericSortPatch
        {
            public static ModConfig config = Configuration<ModConfig>.Load();

            public static void Postfix(PrefabInfo a, PrefabInfo b, ref int __result)
            {
                if (config.IsMixedSortEnabled)
                {
                    int num = a.m_UIPriority.CompareTo(b.m_UIPriority);
                    if (num == 0 && a.m_isCustomContent && b.m_isCustomContent)
                    {
                        num = a.GetLocalizedTitle().CompareTo(b.GetLocalizedTitle());
                    }
                    __result = num;
                }
            }
        }

        // Make vanilla networks sort nicely next to custom ones
        // Used for the roads panel
        [HarmonyPatch(typeof(GeneratedScrollPanel), "ItemsTypeSort")]
        public static class GeneratedScrollPanelItemsTypeSortPatch
        {
            public static ModConfig config = Configuration<ModConfig>.Load();

            public static void Postfix(PrefabInfo a, PrefabInfo b, ref int __result)
            {
                if (config.IsMixedSortEnabled)
                {
                    int num = a.m_UIPriority.CompareTo(b.m_UIPriority);
                    if (num == 0 && a.m_isCustomContent && b.m_isCustomContent)
                    {
                        num = a.GetLocalizedTitle().CompareTo(b.GetLocalizedTitle());
                    }
                    __result = num;
                }
            }

            // Make vanilla networks sort nicely next to custom ones
            // Used for the public transport panels
            [HarmonyPatch(typeof(GeneratedScrollPanel), "ItemsTypeReverseSort")]
            public static class GeneratedScrollPanelItemsTypeReverseSortPatch
            {
                public static ModConfig config = Configuration<ModConfig>.Load();

                public static void Postfix(PrefabInfo a, PrefabInfo b, ref int __result)
                {
                    if (config.IsMixedSortEnabled)
                    {
                        int num = a.m_UIPriority.CompareTo(b.m_UIPriority);
                        if (num == 0 && a.m_isCustomContent && b.m_isCustomContent)
                        {
                            num = a.GetLocalizedTitle().CompareTo(b.GetLocalizedTitle());
                        }
                        __result = num;
                    }
                }
            }
        }
    }
}
