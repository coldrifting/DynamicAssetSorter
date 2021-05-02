using System.IO;
using ICities;
using ColossalFramework.UI;
using CitiesHarmony.API;

namespace DynamicAssetSorter
{
    public class ModSettings : LoadingExtensionBase, IUserMod
    {
        public string Name => "Dynamic Asset Sorter";
        public string Description => "Sort and Hide Asset Icons";

        public void OnEnabled()
        {
            ModConfig.ReadConfig();
            HarmonyHelper.DoOnHarmonyReady(() => DynamicAssetSorter.PatchAll());
        }

        public void OnDisabled()
        {
            if (HarmonyHelper.IsHarmonyInstalled) DynamicAssetSorter.UnpatchAll();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            Reload();
        }

        public static void Reload()
        {
            // Create a new rules config if it doesn't exist
            if (!File.Exists(ModConfig.configPath))
            {
                ModConfig.ResetConfig();
            }

            ModConfig.ReadConfig();
            if (InGame())
            {
                DynamicAssetSorter.Update();
            }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            // Load Setting(s)
            ModConfig config = Configuration<ModConfig>.Load();

            UIHelperBase group_main = helper.AddGroup("Dynamic Asset Sorter");
            group_main.AddCheckbox("Sort vanilla and custom assets interchangeably", config.IsMixedSortEnabled, delegate(bool isEnabled)
            {
                config.IsMixedSortEnabled = isEnabled;
                Configuration<ModConfig>.Save();
                DynamicAssetSorter.Update();
            });
            group_main.AddSpace(10);

            group_main.AddButton($"Reload Settings", () => Reload());
            group_main.AddSpace(10);

            group_main.AddButton("Edit Settings File", () => OpenConfig());
            group_main.AddSpace(40);

            group_main.AddButton("Reset Settings File", () => ResetConfigPrompt());
        }

        public static void OpenConfig()
        {
            // Create a new rules config if it doesn't exist
            if (!File.Exists(ModConfig.configPath))
                ModConfig.ResetConfig();

            // Open the rules config in the default text editor
            System.Diagnostics.Process.Start(ModConfig.configPath);
        }

        private void ResetConfigPrompt()
        {
            ConfirmPanel panel = UIView.library.ShowModal<ConfirmPanel>("ConfirmPanel", delegate (UIComponent component, int response)
            {
                if (response == 1)
                    ModConfig.ResetConfig();
            });
            panel.SetMessage(
                "Dynamic Asset Sorter",
                "This will reset the settings file. " +
                "Any customized settings will be lost. " +
                "Are you sure?");
        }

        public static bool InGame()
        {
            return (LoadingManager.instance.m_loadedEnvironment != null);
        }
    }
}
