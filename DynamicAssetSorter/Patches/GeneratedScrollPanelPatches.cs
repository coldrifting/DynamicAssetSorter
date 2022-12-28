using ColossalFramework;
using ColossalFramework.UI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;

namespace DynamicAssetSorter
{
    class GeneratedScrollPanelPatches
    {
        public static ModConfig config;

        /// <summary>
        /// Make vanilla prefabs sort next to custom ones.
        /// Used for most panels.
        /// </summary>
        [HarmonyPatch(typeof(GeneratedScrollPanel))]
        [HarmonyPatch("ItemsGenericSort")]
        [HarmonyPatch(new Type[] { typeof(PrefabInfo), typeof(PrefabInfo) })]
        public static class GeneratedScrollPanelItemsGenericSortPatch
        {
            public static void Postfix(PrefabInfo a, PrefabInfo b, ref int __result)
            {
                __result = ComparePrefabs(a, b, __result);
            }
        }


        /// <summary>
        /// Make vanilla networks sort nicely next to custom ones.
        /// Used for the roads panel.
        /// </summary>
        [HarmonyPatch(typeof(GeneratedScrollPanel), "ItemsTypeSort")]
        public static class GeneratedScrollPanelItemsTypeSortPatch
        {
            public static void Postfix(PrefabInfo a, PrefabInfo b, ref int __result)
            {
                __result = ComparePrefabs(a, b, __result);
            }

        }


        /// <summary>
        /// Make vanilla networks sort nicely next to custom ones.
        /// Used for the public transport panels.
        /// </summary>
        [HarmonyPatch(typeof(GeneratedScrollPanel), "ItemsTypeReverseSort")]
        public static class GeneratedScrollPanelItemsTypeReverseSortPatch
        {
            public static void Postfix(PrefabInfo a, PrefabInfo b, ref int __result)
            {
                __result = ComparePrefabs(a, b, __result);
            }
        }


        /// <summary>
        /// Method defined here to reduce code duplication above.
        /// </summary>
        private static int ComparePrefabs(PrefabInfo a, PrefabInfo b, int originalValue)
        {
            if (config == null)
            {
                config = Configuration<ModConfig>.Load();
            }

            if (config.IsMixedSortEnabled)
            {
                int num = a.m_UIPriority.CompareTo(b.m_UIPriority);
                if (num == 0 && a.m_isCustomContent && b.m_isCustomContent)
                {
                    num = a.GetLocalizedTitle().CompareTo(b.GetLocalizedTitle());
                }
                return num;
            }
            else
            {
                return originalValue;
            }
        }

        /// <summary>
        /// Override the ShowAssets method to make sure our hidden icons stay hidden
        /// </summary>
        [HarmonyPatch(typeof(GeneratedScrollPanel), "ShowAssetsThatShouldBeShown")]
        public static class GeneratedScrollPanelShowAssetsThatShouldBeShownPatch
        {
            public static ModConfig config = Configuration<ModConfig>.Load();

            public static bool Prefix(GeneratedScrollPanel __instance)
            {
                // Get Private field m_scrollablePanel
                BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
                FieldInfo fi_name = typeof(GeneratedScrollPanel).GetField("m_ScrollablePanel", flags);
                UIScrollablePanel m_ScrollablePanel = (UIScrollablePanel)fi_name.GetValue(__instance);

                // Get Private Method ShouldAssetBeVisible(Object obj)
                MethodInfo md_name = typeof(GeneratedScrollPanel).GetMethod("ShouldAssetBeVisible", flags);

                foreach (UIComponent component in m_ScrollablePanel.components)
                {
                    UIButton button = component as UIButton;
                    if (!(button != null) || button.objectUserData == null)
                    {
                        continue;
                    }

                    // Hide icons by skipping them if they belong to a specified panel.
                    if (ModConfig.hiddenIcons.TryGetValue(button.name, out ModConfig.IconInfo iconInfo))
                    {
                        if (button.parent.parent != null && button.parent.parent.name == iconInfo.ParentPanel)
                        {
                            button.isVisible = false;
                            continue;
                        }
                    }

                    bool shouldBeVisible = (bool)typeof(GeneratedScrollPanel).GetMethod("ShouldAssetBeVisible", flags).Invoke(__instance, new object[] { button.objectUserData });
                    if (!shouldBeVisible || button.isVisible)
                    {
                        continue;
                    }

                    button.isVisible = true;
                    ValueAnimator.Animate(delegate (float val)
                    {
                        if (button != null)
                        {
                            button.opacity = val;
                        }
                    }, new AnimatedFloat((!shouldBeVisible) ? 1f : 0f, (!shouldBeVisible) ? 0f : 1f, 0.2f), delegate
                    {
                        button.isVisible = shouldBeVisible;
                    });
                }

                // Skip the rest of the original method.
                return false;
            }
        }
    }
}
