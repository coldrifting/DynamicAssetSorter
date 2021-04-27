using System;
using System.IO;
using UnityEngine;
using ICities;
using ColossalFramework.IO;
using ColossalFramework.UI;
using static ColossalFramework.UI.UIView;

namespace DynamicAssetSorter
{
    public class ModSettings : IUserMod
    {
        public string Name => "Dynamic Asset Sorter";
        public string Description => "Adjust Asset UI Priority on the fly";

        public static string configFileName = "DynamicAssetSorterConfig.cfg";
        public static string configFilePath = DataLocation.localApplicationData;
        public static string configFileFullPath = configFilePath + "\\" + configFileName;

        public void OnSettingsUI(UIHelperBase helper)
        {
            // Main Header
            UIHelperBase group_main = helper.AddGroup("Dynamic Asset Sorter - Adjust UI sorting of assets");

            // Edit Config file with default text editor
            group_main.AddButton("Edit Config", () =>
            {
                System.Diagnostics.Process.Start(configFileFullPath);
            });
            group_main.AddSpace(4);

            // Reload the config
            group_main.AddButton($"Reload Config", () =>
            {
                // Make sure we are in game and not a menu.
                if (!GameAreaManager.exists || !SimulationManager.exists || LoadingManager.instance.m_loadedEnvironment == null)
                {
                    ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
                    panel.SetMessage("Dynamic Asset Sorter", "Settings can only be reloaded ingame.", false);
                    return;
                }

                // Find the class
                DynamicAssetSorter dynamicAssetSorter = GameObject.FindObjectOfType<DynamicAssetSorter>();
                if (dynamicAssetSorter != null)
                    dynamicAssetSorter.Sort();
            });
            group_main.AddSpace(4);

            // Reset the config to the default
            UIHelperBase group_reset = helper.AddGroup("Reset Config");
            group_reset.AddButton("Reset Config File", () =>
            {
                // Assign Delegate for Dialog box response
                ModalPoppedReturnCallback resetConfigBtnDel = ResetConfigButtonClicked;
                ConfirmPanel panel = UIView.library.ShowModal<ConfirmPanel>("ConfirmPanel", resetConfigBtnDel);
                panel.SetMessage(
                    "Dynamic Asset Sorter",
                    "This will apply an example config file. " +
                    "Any custom sorting settings will be lost. Are you sure?");

                // Inline Delegate Method for 
                void ResetConfigButtonClicked(UIComponent test, int result)
                {
                    try
                    {
                        if (Convert.ToBoolean(result))
                        {
                            ResetConfigFile(configFileFullPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Log("DynamicAssetSorter - ResetConfigButtonClicked()");
                        Debug.Log(ex.Message);
                    }
                }

            });
        }

        public void ResetConfigFile(string configFile)
        {
            try
            {
                StreamWriter writer = new StreamWriter(configFile);
                writer.WriteLine("# Config File Format: Sorting Priority, Type (Network or Building), Asset Name");
                writer.WriteLine("# Example: 1000, Network, HighwayRamp");
                writer.WriteLine("# Use a pound sign at the start of a line to comment it out");
                writer.WriteLine("# The mod will ignore any lines that aren't properly formated");
                writer.WriteLine("# Extra whitespace for padding is okay, but don't quote asset names");
                writer.WriteLine("# Higher sorting values will sort after lower ones, and you can also use negative values");
                writer.WriteLine("# You can sort both workshop and vanilla content, but vanilla content always sorts before workshop content");
                writer.WriteLine("# One way to find asset names is to install Mod Tools, press Ctrl+R, and hover over asset icons");
                writer.WriteLine("");
                writer.WriteLine("# Electricity");
                writer.WriteLine("0050, Building, Wind Turbine");
                writer.WriteLine("0500, Network,  Power Line");
                writer.WriteLine("1000, Building, 816731978.Large Coal Power Plant_Data");
                writer.Close();
            }
            catch (Exception ex)
            {
                Debug.Log("Unable to write DynamicAssetSorter config file");
                Debug.Log(ex.Message);
            }
        }

    }
}
