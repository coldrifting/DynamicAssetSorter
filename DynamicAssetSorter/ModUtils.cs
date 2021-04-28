using ICities;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DynamicAssetSorter
{
    [ConfigurationPath("DynamicAssetSorter.xml")]
    public class ModConfig
    {
        public bool IsMixedSortEnabled { get; set; } = false;
    }

    public class PrefabRule
    {
        public int Priority;
        public string PrefabType;
        public string PrefabName;

        public PrefabRule(int priority, string prefabType, string prefabName)
        {
            this.Priority = priority;
            this.PrefabName = prefabName;
            this.PrefabType = prefabType;
        }
    }

    public class ModUtils
    {
        // Resets the config file to a blank default
        public static void ResetRulesConfig(string rulesConfigPath)
        {
            try
            {
                StreamWriter writer = new StreamWriter(rulesConfigPath);
                writer.WriteLine("# Sorting Rules Configuration Format: Sorting Priority, Type (Network, Building, or Transport (i.e. TransportLine)), Asset Prefab Name");
                writer.WriteLine("# Higher sorting values will sort after lower ones, and you can also use negative values");
                writer.WriteLine("# You can sort both workshop and vanilla content, but by default vanilla assets always sort before any workshop assets");
                writer.WriteLine("# One way to find Asset Prefab Names is to install Mod Tools, press Ctrl+R, and hover over asset icons");
                writer.WriteLine("# Examples:");
                writer.WriteLine("# 0001, Network,   Basic Road");
                writer.WriteLine("# 1500, Network,   Power Line");
                writer.WriteLine("# 1000, Building,  816731978.Large Coal Power Plant_Data");
                writer.WriteLine("# 0000, Transport, Bus");
                writer.Close();
            }
            catch (Exception ex)
            {
                Debug.Log("Unable to write DynamicAssetSorter Rules Config File");
                Debug.Log(ex.Message);
            }
        }

        // Reads and parses the config file
        public static List<PrefabRule> ReadRulesConfig(string rulesConfigPath)
        {
            // Create a list of rules
            List<PrefabRule> prefabRules = new List<PrefabRule>();
            try
            {
                // Read the Config File
                string line;
                StreamReader reader = new StreamReader(rulesConfigPath);
                while ((line = reader.ReadLine()) != null)
                {
                    // Skip comments and empty lines
                    if (line.StartsWith("#") || line.Equals(""))
                        continue;

                    // Split each line into 3 parts
                    string[] delimiter = { ", " };
                    string[] splitLine = line.Split(delimiter, 3, StringSplitOptions.RemoveEmptyEntries);

                    // Skip a line if it's first value can't be converted to an integer
                    int priority;
                    if (!int.TryParse(splitLine[0].Trim(), out priority))
                        continue;

                    // Skip an entry if it's second value isn't a valid prefab type
                    string prefabType = splitLine[1].Trim();
                    if (!prefabType.Equals("Network") && 
                        !prefabType.Equals("Building") && 
                        !prefabType.Equals("Transport"))
                        continue;

                    // The final part of the line should be the prefab name
                    string prefabName = splitLine[2].Trim();

                    // Apply the change
                    prefabRules.Add(new PrefabRule(priority, prefabType, prefabName));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Debug.Log("Unable to read DynamicAssetSorter Rules Config File");
                Debug.Log(ex.Message);
            }
            return prefabRules;
        }

        public static bool IsInGame()
        {
            return (LoadingManager.instance.m_loadedEnvironment != null);
        }
    }
}
