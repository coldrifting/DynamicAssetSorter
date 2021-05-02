using System.Collections.Generic;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;
using HarmonyLib;
using System;
using System.Linq;

namespace DynamicAssetSorter
{
    public class DynamicAssetSorter
    {
        private const string HarmonyId = "coldrifting.DynamicAssetSorter";
        private static bool patched;

        private static List<UIButton> currentHiddenIcons = new List<UIButton>();

        public static void PatchAll()
        {
            if (patched)
            {
                return;
            }

            patched = true;

            var harmony = new Harmony(HarmonyId);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static void UnpatchAll()
        {
            if (!patched)
            {
                return;
            }
            
            var harmony = new Harmony(HarmonyId);
            harmony.UnpatchAll(HarmonyId);
            patched = false;
        }

        public static void Update()
        {
            SortPrefabs();
            ResetIcons();
            HideIcons();
            RefreshUI();
        }

        public static void SortPrefabs()
        {
            foreach(ModConfig.SortRule sortRule in ModConfig.prefabRules)
            {
                PrefabInfo prefab = null;
                switch (sortRule.Type)
                {
                    case "Network":
                        prefab = PrefabCollection<NetInfo>.FindLoaded(sortRule.Name);
                        break;
                    case "Building":
                        prefab = PrefabCollection<BuildingInfo>.FindLoaded(sortRule.Name);
                        break;
                    case "Transport":
                        prefab = PrefabCollection<TransportInfo>.FindLoaded(sortRule.Name);
                        break;
                    default:
                        break;
                }
                if (prefab != null)
                {
                    prefab.m_UIPriority = sortRule.Priority;
                }
            }
        }

        public static void ResetIcons()
        {
            foreach (UIButton button in currentHiddenIcons)
            {
                button.Show();
            }
            currentHiddenIcons.Clear();
        }

        public static void HideIcons()
        {
            UIButton[] buttons = UIView.FindObjectsOfType<UIButton>();
            foreach (ModConfig.IconInfo iconInfo in ModConfig.hiddenIcons)
            {
                foreach (UIButton button in buttons)
                {
                    if (button.name == iconInfo.Name && button.parent.parent.name == iconInfo.Grandparent)
                    {
                        button.Hide();
                        currentHiddenIcons.Add(button);
                    }
                }
            }
        }

        public static void RefreshUI()
        {
            MainToolbar mainToolbar = GameObject.FindObjectOfType<MainToolbar>();
            if (mainToolbar != null)
            {
                mainToolbar.RefreshPanel();
            }
        }
    }
}
