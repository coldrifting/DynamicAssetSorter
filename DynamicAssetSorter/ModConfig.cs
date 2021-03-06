using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using ColossalFramework.IO;
using ColossalFramework.UI;

namespace DynamicAssetSorter
{
    [ConfigurationPath("DynamicAssetSorter.xml")]
    public class ModConfig
    {
        public const string modName = "DynamicAssetSorter";
        public static string configFolder = DataLocation.localApplicationData;
        public static string configName = modName + ".ini";
        public static string configPath = configFolder + "\\" + configName;

        public static List<SortRule> prefabRules = new List<SortRule>();
        public static List<IconInfo> hiddenIcons = new List<IconInfo>();

        // Instance variable for XML Config File
        public bool IsMixedSortEnabled { get; set; } = false;

        public class SortRule
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public int Priority { get; set; }

            public SortRule(string name, string type, int priority)
            {
                this.Name = name;
                this.Type = type;
                this.Priority = priority;
            }
        }

        public class IconInfo
        {
            public string Name { get; set; }
            public string Grandparent { get; set; }

            public IconInfo(string name, string grandparent)
            {
                this.Name = name;
                this.Grandparent = grandparent;
            }
        }

        // Reads and parses the config file
        public static void ReadConfig()
        {
            // Reset the config storage
            prefabRules.Clear();
            hiddenIcons.Clear();

            // Store current line for exception handling
            int currentLine = 0;
            try
            {
                // Read the Config File
                string line;
                StreamReader reader = new StreamReader(configPath);
                while ((line = reader.ReadLine()) != null)
                {
                    currentLine++;

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
                            hiddenIcons.Add(new IconInfo(iconName, iconGrandparent));
                            continue;
                        }

                        // Skip a line if it's first value can't be converted to an integer
                        int priority;
                        if (!int.TryParse(splitLine[0].Trim(), out priority))
                        {
                            Debug.Log($"{modName}: Invalid config entry. Entries must start with a number or the prefix \"HideIcon, \"");
                            continue;
                        }

                        // The last part of the line will be the prefab name
                        string prefabName = splitLine[2].Trim();

                        // Assign a line to the correct dictionary
                        string prefabType = splitLine[1].Trim();
                        if (prefabType == "Network" ||
                            prefabType == "Building" ||
                            prefabType == "Transport" ||
                            prefabType == "Tree" ||
                            prefabType == "Prop")
                        {
                            prefabRules.Add(new SortRule(prefabName, prefabType, priority));
                        }
                        else
                        {
                            Debug.Log($"{modName}: Invalid prefab type: \"{prefabType}\" for prefab \"{prefabName}\" in config file.");
                        }
                    }
                    else
                    {
                        Debug.Log($"{modName}: \"{line}\" is an invalid config entry. Check that you aren't missing any commas.");
                        Debug.Log($"{modName}: Example Format: \"[Number], [PrefabType], [PrefabName]\"");
                    }
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                if (IsUIAvailable())
                {
                    ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
                    panel.SetMessage(
                        "Dynamic Asset Sorter", 
                        "Unable to read config file! \n" + 
                        "Please check line number " + currentLine + "\n " + 
                        ex.Message, false);
                }
                Debug.Log($"{modName}: Unable to read config file!");
                Debug.Log(ex.Message);
            }
        }


        // Resets the config file to a blank default
        public static void ResetConfig()
        {
            try
            {
                StreamWriter writer = new StreamWriter(configPath);
                writer.WriteLine("# Dynamic Asset Sorter configuration file");
                writer.WriteLine("# See the steam workshop page for configuration help");
                writer.Close();
            }
            catch (Exception ex)
            {
                if (IsUIAvailable())
                {
                    ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
                    panel.SetMessage("Dynamic Asset Sorter", "Unable to write to config file! \n" + ex.Message, false);
                }
                Debug.Log($"{modName}: Unable to write to config file!");
                Debug.Log(ex.Message);
            }
        }

        public static bool IsUIAvailable()
        {
            return UIView.GetAView() != null;
        }
    }
}
