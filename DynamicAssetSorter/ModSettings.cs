using ICities;
using CitiesHarmony.API;

namespace DynamicAssetSorter
{
    public class ModSettings : LoadingExtensionBase, IUserMod
    {
        public string Name => "Dynamic Asset Sorter";
        public string Description => "Sort and Hide Asset Icons";

        /// <summary>
        /// Harmony helper functions for mod enabling.
        /// </summary>
        public void OnEnabled()
        {
            HarmonyHelper.DoOnHarmonyReady(() => DynamicAssetSorter.PatchAll());
        }


        /// <summary>
        /// Harmony helper functions for mod disabling.
        /// </summary>
        public void OnDisabled()
        {
            if (HarmonyHelper.IsHarmonyInstalled) DynamicAssetSorter.UnpatchAll();
        }


        /// <summary>
        /// Runs the mod's main function on level loading.
        /// </summary>
        /// <param name="mode"></param>
        public override void OnLevelLoaded(LoadMode mode)
        {
            // Disable the mod in the asset editors
            if (mode == LoadMode.NewAsset ||
                mode == LoadMode.LoadAsset ||
                mode == LoadMode.NewMap ||
                mode == LoadMode.LoadMap ||
                mode == LoadMode.NewTheme ||
                mode == LoadMode.LoadTheme)
                return;

            DynamicAssetSorter.Update();
        }


        /// <summary>
        /// Sets up settings GUI for the mod.
        /// </summary>
        public void OnSettingsUI(UIHelperBase helper)
        {
            // Load Setting(s)
            ModConfig config = Configuration<ModConfig>.Load();

            UIHelperBase group_main = helper.AddGroup("Dynamic Asset Sorter");
            group_main.AddCheckbox("Enable Mixed Sorting of Vanilla/Custom Assets", config.IsMixedSortEnabled, delegate(bool isEnabled)
            {
                config.IsMixedSortEnabled = isEnabled;
                Configuration<ModConfig>.Save();

                // Only update in-game
                if (LoadingManager.instance.m_loadedEnvironment != null)
                    DynamicAssetSorter.Update();
            });
            group_main.AddSpace(10);

            group_main.AddButton($"Reload Settings", () => DynamicAssetSorter.Update());
            group_main.AddSpace(10);

            group_main.AddButton("Edit Settings File", () => ModConfig.EditConfigFile());
            group_main.AddSpace(30);

            group_main.AddButton("Export Unlocked Prefab Settings to File", () => ModConfig.ExportPrefabInfo());
        }
    }
}
