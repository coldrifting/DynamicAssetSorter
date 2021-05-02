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

            // Override show assets method to make sure our hidden icons stay hidden
            [HarmonyPatch(typeof(GeneratedScrollPanel), "ShowAssetsThatShouldBeShown")]
            public static class GeneratedScrollPanelShowAssetsThatShouldBeShownPatch
            {
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

                        // Don't show icons that we want to hide
                        ModConfig.IconInfo firstMatch = ModConfig.hiddenIcons.FirstOrDefault(i => i.Name == button.name);
                        if (firstMatch != null)
                        {
                            // Double check the button has a grandparent before accessing
                            if (button.parent.parent != null)
                            {
                                // Only skip showing a button if it belongs to the panel specified in the config
                                if (firstMatch.Grandparent == button.parent.parent.name)
                                {
                                    continue;
                                }
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

                    // Skip
                    return false;
                }
            }
        }
    }
}
