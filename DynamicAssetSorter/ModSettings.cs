using ICities;
using ColossalFramework.IO;
using ColossalFramework.UI;
using CitiesHarmony.API;
using System.Collections.Generic;
using System.IO;

namespace DynamicAssetSorter
{
    public class ModSettings : LoadingExtensionBase, IUserMod
    {
        public string Name => "Dynamic Asset Sorter";
        public string Description => "Adjust Asset UI Priority on the fly";

        internal static string rulesFolder = DataLocation.localApplicationData;
        internal static string rulesFileName = "DynamicAssetSorterRules.ini";
        internal static string rulesConfigPath = rulesFolder + "\\" + rulesFileName;

        internal static List<PrefabRule> prefabRules;

        public void OnEnabled()
        {
            HarmonyHelper.DoOnHarmonyReady(() => DynamicAssetSorter.PatchAll());
        }

        public void OnDisabled()
        {
            if (HarmonyHelper.IsHarmonyInstalled) DynamicAssetSorter.UnpatchAll();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            Reload(rulesConfigPath);
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            // Load Setting(s)
            ModConfig config = Configuration<ModConfig>.Load();

            UIHelperBase group_main = helper.AddGroup("Dynamic Asset Sorter - Adjust UI sorting of assets");
            group_main.AddCheckbox("Mix Vanilla and Custom Assets together when sorting", config.IsMixedSortEnabled, delegate(bool isEnabled)
            {
                config.IsMixedSortEnabled = isEnabled;
                Configuration<ModConfig>.Save();
                DynamicAssetSorter.RefreshUI();
            });
            group_main.AddSpace(10);

            UIHelperBase group_rules = helper.AddGroup("Sorting Rules Configuration");
            group_rules.AddButton($"Reload Sorting Rules", () => Reload(rulesConfigPath));
            group_rules.AddSpace(10);

            group_rules.AddButton("Edit Sorting Rules Config File", () => OpenRulesConfig(rulesConfigPath));
            group_rules.AddSpace(30);

            group_rules.AddButton("Reset Sorting Rules Config File", () => ResetRulesConfigUI());
        }

        public static void Reload(string rulesConfigPath)
        {
            prefabRules = ModUtils.ReadRulesConfig(rulesConfigPath);
            if (ModUtils.IsInGame())
                DynamicAssetSorter.Update();
        }

        public static void OpenRulesConfig(string rulesConfigPath)
        {
            // Create a new rules config if it doesn't exist
            if (!File.Exists(rulesConfigPath))
                ModUtils.ResetRulesConfig(rulesConfigPath);

            // Open the rules config in the default text editor
            System.Diagnostics.Process.Start(rulesConfigPath);
        }

        private void ResetRulesConfigUI()
        {
            ConfirmPanel panel = UIView.library.ShowModal<ConfirmPanel>("ConfirmPanel", delegate (UIComponent component, int response)
            {
                if (response == 1)
                    ModUtils.ResetRulesConfig(rulesConfigPath);
            });
            panel.SetMessage(
                "Dynamic Asset Sorter",
                "This will reset the rules configuration file.    " +
                "Custom sorting rules will be lost. " +
                "Are you sure?");
        }
    }
}
