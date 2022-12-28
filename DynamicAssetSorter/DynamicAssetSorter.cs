using System.Collections.Generic;
using System.Reflection;
using ColossalFramework.UI;
using UnityEngine;
using HarmonyLib;

namespace DynamicAssetSorter
{
    public class DynamicAssetSorter
    {
        private const string HarmonyId = "coldrifting.DynamicAssetSorter";
        private static bool patched;

        private static readonly List<UIButton> CurrentHiddenIcons = new List<UIButton>();

        /// <summary>
        /// Enables the Harmony patches.
        /// </summary>
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


        /// <summary>
        /// Disables the Harmony patches.
        /// </summary>
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


        /// <summary>
        /// The main processes that happen each time the mod is run.
        /// </summary>
        public static void Update()
        {
            if (ModConfig.LoadConfigFile())
            {
                SortPrefabs();
                ResetIcons();
                HideIcons();
                RefreshUI();
            }
        }


        /// <summary>
        /// Assigns new UI priority values according to the config file.
        /// </summary>
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

                    case "Tree":
                        prefab = PrefabCollection<TreeInfo>.FindLoaded(sortRule.Name);
                        break;

                    case "Prop":
                        prefab = PrefabCollection<PropInfo>.FindLoaded(sortRule.Name);
                        break;

                    default:
                        break;
                }

                if (prefab != null)
                    prefab.m_UIPriority = sortRule.UIOrder;
            }
        }


        /// <summary>
        /// Makes all previously hidden icons reappear.
        /// </summary>
        public static void ResetIcons()
        {
            foreach (UIButton button in CurrentHiddenIcons)
            {
                button.Show();
            }
            CurrentHiddenIcons.Clear();
        }


        /// <summary>
        /// Hides the icons chosen in the config.
        /// </summary>
        public static void HideIcons()
        {
            UIButton[] buttons = Object.FindObjectsOfType<UIButton>();
            foreach (ModConfig.IconInfo iconInfo in ModConfig.hiddenIcons.Values)
            {
                foreach (UIButton button in buttons)
                {
                    if (button.name == iconInfo.Name && button.parent.parent.name == iconInfo.ParentPanel)
                    {
                        button.Hide();
                        CurrentHiddenIcons.Add(button);
                    }
                }
            }
        }


        /// <summary>
        /// Makes the UI changes take effect.
        /// </summary>
        public static void RefreshUI()
        {
            MainToolbar mainToolbar = Object.FindObjectOfType<MainToolbar>();
            if (mainToolbar != null)
            {
                mainToolbar.RefreshPanel();
            }
        }
    }
}
