using System.IO;
using ICities;
using ColossalFramework.UI;
using CitiesHarmony.API;

namespace DynamicAssetSorter
{
    public class ModSettings : LoadingExtensionBase, IUserMod
    {
        public string Name => "Dynamic Asset Sorter";
        public string Description => "Adjust Asset UI Priority on the fly";

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
            ModConfig.ReadConfig();
            if (InGame())
                DynamicAssetSorter.Update();
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            // Load Setting(s)
            ModConfig config = Configuration<ModConfig>.Load();

            UIHelperBase group_main = helper.AddGroup("Dynamic Asset Sorter - Adjust UI sorting of assets");
            group_main.AddCheckbox("Sort vanilla and custom assets interchangeably", config.IsMixedSortEnabled, delegate(bool isEnabled)
            {
                config.IsMixedSortEnabled = isEnabled;
                Configuration<ModConfig>.Save();
                DynamicAssetSorter.Update();
            });
            group_main.AddSpace(10);

            UIHelperBase group_rules = helper.AddGroup("Sorting Rules Configuration");
            group_rules.AddButton($"Reload Settings", () => Reload());
            group_rules.AddSpace(10);

            group_rules.AddButton("Edit Settings File", () => OpenConfig());
            group_rules.AddSpace(30);

            group_rules.AddButton("Reset Settings File", () => ResetConfigPrompt());
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
                "This will reset the rules configuration file.    " +
                "Custom sorting rules will be lost. " +
                "Are you sure?");
        }

        public static bool InGame()
        {
            return (LoadingManager.instance.m_loadedEnvironment != null);
        }
    }
}
