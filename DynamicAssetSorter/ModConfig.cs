using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using ColossalFramework.IO;
using ColossalFramework.UI;
using System.Linq;

namespace DynamicAssetSorter
{
    [ConfigurationPath("DynamicAssetSorter.xml")]
    public class ModConfig
    {
        public static readonly string ModName = "DynamicAssetSorter";
        public static readonly string ConfigFolder = DataLocation.localApplicationData;
        public static readonly string ConfigFileName = ModName + ".ini";
        public static readonly string ExportFileName = ModName + "_EXPORT.ini";
        public static readonly string ConfigFilePath = ConfigFolder + "\\" + ConfigFileName;
        public static readonly string ExportFilePath = ConfigFolder + "\\" + ExportFileName;

        public static int prefabNameMaxLength = 0;
        public static readonly List<SortRule> prefabRules = new List<SortRule>();
        public static readonly Dictionary<string, IconInfo> hiddenIcons = new Dictionary<string, IconInfo>();

        public static readonly string[] PanelsToSkip = new string[]
        {
            "LandscapingDisastersPanel",
            "FindItDefaultPanel"
        };

        // Instance variable for XML Config File
        public bool IsMixedSortEnabled { get; set; } = false;

        /// <summary>
        /// Nested inner class useful for passing info between config files and the game, in both directions.
        /// </summary>
        public struct SortRule
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public int UIOrder { get; set; }
            public string UICategory { get; set; }

            public SortRule(string name, string type, int uiOrder, string uiCategory)
            {
                Name = name;
                Type = type;
                UIOrder = uiOrder;
                UICategory = uiCategory;
            }
        }


        /// <summary>
        /// Anther useful nested inner class, this one is for hiding icons from a specified UI panel.
        /// </summary>
        public struct IconInfo
        {
            public string Name { get; set; }
            public string ParentPanel { get; set; }

            public IconInfo(string name, string parentPanel)
            {
                Name = name;
                ParentPanel = parentPanel;
            }
        }


        /// <summary>
        /// Reads and parses the config file.
        /// </summary>
        /// <returns>True if the config was successfully updated, false otherwise</returns>
        public static bool LoadConfigFile()
        {
            if (!IsInGame())
                return false;

            // Reset the config storage
            prefabRules.Clear();
            hiddenIcons.Clear();

            // Don't do anything if the config file doesn't exist
            if (!File.Exists(ConfigFilePath))
                return false;

            // Store current line for exception handling
            int currentLine = 0;
            try
            {
                // Read the Config File
                string line;
                StreamReader reader = new StreamReader(ConfigFilePath);
                while ((line = reader.ReadLine()) != null)
                {
                    currentLine++;

                    line = line.Trim();

                    // Skip comments and empty lines
                    if (line.StartsWith("#") || line.Equals(""))
                        continue;

                    // Split the line into 3 parts
                    string[] delimiter = { ", " };
                    string[] splitLine = line.Split(delimiter, 3, StringSplitOptions.RemoveEmptyEntries);
                    if (splitLine.Length == 3)
                    {
                        if (line.StartsWith("HideIcon", StringComparison.InvariantCultureIgnoreCase))
                        {
                            string iconName = splitLine[2].Trim();
                            string iconGrandparent = splitLine[1].Trim();
                            hiddenIcons[iconName] = new IconInfo(iconName, iconGrandparent);
                            continue;
                        }

                        // Skip a line if it's first value can't be converted to an integer
                        if (!int.TryParse(splitLine[0].Trim(), out int priority))
                        {
                            Debug.Log($"{ModName}: Invalid config entry. Entries must start with a number or the prefix \"HideIcon, \"");
                            continue;
                        }

                        // The last part of the line will be the prefab name, excluding any trailing line comments
                        string prefabName = splitLine[2].Replace("#.*$", "").Trim();

                        // Assign a line to the correct dictionary
                        string prefabType = splitLine[1].Trim();
                        if (prefabType == "Network" ||
                            prefabType == "Building" ||
                            prefabType == "Transport" ||
                            prefabType == "Tree" ||
                            prefabType == "Prop")
                        {
                            prefabRules.Add(new SortRule(prefabName, prefabType, priority, ""));
                        }
                        else
                        {
                            Debug.Log($"{ModName}: Invalid prefab type: \"{prefabType}\" for prefab \"{prefabName}\" in config file.");
                        }
                    }
                    else
                    {
                        Debug.Log($"{ModName}: \"{line}\" is an invalid config entry. Check that you aren't missing any commas.");
                        Debug.Log($"{ModName}: Example Format: \"[Number], [PrefabType], [PrefabName]\"");
                    }
                }
                reader.Close();

                return true;
            }
            catch (Exception ex)
            {
                string errorMsg = "Unable to read config file! \n" +
                                  "Please check line number " + 
                                  currentLine;
                DisplayException(ex, errorMsg);

                return false;
            }
        }


        /// <summary>
        /// Opens the config file in the default text editor.
        /// Creates a new file if the file does not exist.
        /// </summary>
        public static void EditConfigFile()
        {
            // Create a new rules config if it doesn't exist
            if (!File.Exists(ConfigFilePath))
                ResetConfigFile();

            // Open the rules config in the default text editor
            System.Diagnostics.Process.Start(ConfigFilePath);
        }


        /// <summary>
        /// Gets the name, priority, and category of all currently unlocked items.
        /// In order for this to be accurate, you must run this before making any changes.
        /// This also means that your Dynamic Asset Sorter config file should be empty before starting the game.
        /// </summary>
        public static void ExportPrefabInfo()
        {
            if (!IsInGame())
                return;

            prefabNameMaxLength = 0;
            List<SortRule> prefabRules = new List<SortRule>();
            try
            {
                UITabContainer tsContainer = GameObject.Find("TSContainer").GetComponent<UITabContainer>();

                // TSContainer : UITabContainer
                if (tsContainer is null)
                    return;

                // e.g. PublicTransportPanel : UIPanel;
                foreach (UIPanel categoryPanel in tsContainer.components.Cast<UIPanel>())
                {
                    // GTSContainer : UITabContainer
                    UITabContainer gtsContainer = categoryPanel.GetComponentInChildren<UITabContainer>();

                    if (gtsContainer is null)
                        continue;

                    // e.g. PublicTransportBusPanel : UIPanel
                    foreach (UIPanel subCategoryPanel in gtsContainer.components.Cast<UIPanel>())
                    {
                        string category = subCategoryPanel.name;

                        // Skip some unique panels
                        if (PanelsToSkip.Contains(category))
                            continue;

                        UIScrollablePanel scrollablePanel = subCategoryPanel.Find("ScrollablePanel") as UIScrollablePanel;

                        foreach (UIButton prefabButton in scrollablePanel.GetComponentsInChildren<UIButton>())
                        {
                            if (prefabButton.objectUserData is PrefabInfo info)
                            {
                                if (info.name.Length > prefabNameMaxLength)
                                    prefabNameMaxLength = info.name.Length;

                                string type = "(Default)";
                                if (info is BuildingInfo)
                                    type = "Building";
                                else if (info is NetInfo)
                                    type = "Network";
                                else if (info is TransportInfo)
                                    type = "Transport";
                                else if (info is PropInfo)
                                    type = "Prop";
                                else if (info is TreeInfo)
                                    type = "Tree";

                                SortRule newRule = new SortRule(info.name, type, info.m_UIPriority, category);
                                prefabRules.Add(newRule);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"{ModConfig.ModName}: An issue occurred while getting default sorting rules: {ex.Message}");
            }

            try
            {
                StreamWriter writer = new StreamWriter(ExportFilePath);
                writer.WriteLine("# Exported Prefabs");

                foreach (SortRule s in prefabRules)
                {
                    writer.WriteLine($"{s.UIOrder:00000;-0000}, {s.Type + ',',-9} {s.Name.PadRight(prefabNameMaxLength, ' ')} # {s.UICategory} ");
                }

                writer.Close();
            }
            catch (Exception ex)
            {
                DisplayException(ex, "Unable to export default sorting rules to a file");
            }

            // Open the rules config in the default text editor
            if (File.Exists(ExportFilePath))
                System.Diagnostics.Process.Start(ExportFilePath);
        }


        /// <summary>
        /// Resets the config file to a blank default
        /// </summary>
        public static void ResetConfigFile()
        {
            try
            {
                StreamWriter writer = new StreamWriter(ConfigFilePath);
                writer.WriteLine("# Dynamic Asset Sorter configuration file");
                writer.WriteLine("# See the steam workshop page for configuration help");
                writer.Close();
            }
            catch (Exception ex)
            {
                DisplayException(ex, "Unable to write to config file");
            }
        }


        /// <summary>
        /// Helper method for displaying exceptions on both the log and as a pop up window
        /// </summary>
        private static void DisplayException(Exception ex, string userFriendlyMsg)
        {
            Debug.Log($"{ModName}: {userFriendlyMsg}");
            Debug.Log(ex.Message);

            // Check that the UI is available before displaying an exception pop-up box
            if (UIView.GetAView() == null)
                return;

            ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
            panel.SetMessage("Dynamic Asset Sorter", $"{userFriendlyMsg} \n {ex.Message}", false);
        }


        /// <summary>
        /// Checks if the user is currently in a loaded game, and
        /// tries to display a pop-up if they are not.
        /// </summary>
        /// <returns>True if the user is inside a loaded game, false otherwise</returns>
        private static bool IsInGame()
        {
            bool isInGame = LoadingManager.instance.m_loadedEnvironment != null;

            // Check that the UI is available before displaying an exception pop-up box
            if (!isInGame && UIView.GetAView() != null)
            {
                ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
                panel.SetMessage("Dynamic Asset Sorter", "You must be inside a loaded save game to export or edit prefab information.", false);
            }

            return isInGame;
        }
    }
}
