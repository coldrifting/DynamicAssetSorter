using System;
using System.IO;
using ColossalFramework.IO;
using ColossalFramework.UI;
using ICities;
using UnityEngine;
using static ColossalFramework.UI.UIView;

namespace DynamicAssetSorter
{
    public class DynamicAssetSorter : MonoBehaviour
    {
        public static bool isGameLoaded = false;

        public void Start()
        {
            try
            {
                Sort();
            }
            catch (Exception ex)
            {
                Debug.Log("Dynamic Asset Sorter - Start() - " + ex.Message);
            }
        }

        public void Sort()
        {
            ApplyConfig(ModSettings.configFileFullPath);
            RefreshUI();
        }

        public void ApplyConfig(string configFile)
        {
            try
            {
                // Find the class
                DynamicAssetSorter dynamicAssetSorter = GameObject.FindObjectOfType<DynamicAssetSorter>();

                string line;
                StreamReader reader = new StreamReader(configFile);
                while ((line = reader.ReadLine()) != null)
                {
                    // Skip comments and empty lines
                    if (line.StartsWith("#") || line.Equals(""))
                        continue;

                    // Split each line into 3 parts
                    string[] delimiter = { ", " };
                    string[] splitLine = line.Split(delimiter, 3, System.StringSplitOptions.RemoveEmptyEntries);

                    // Skip a line if it's first value can't be converted to an integer
                    int priority;
                    if (!int.TryParse(splitLine[0], out priority))
                        continue;

                    // Skip an entry if it's second value isn't a valid prefab type
                    string prefabType = splitLine[1].Trim();
                    if (!prefabType.Equals("Network") && !prefabType.Equals("Building"))
                        continue;

                    // The final part of the line should be the prefab name
                    string prefabName = splitLine[2].Trim();

                    // Apply the change
                    dynamicAssetSorter.SetUIPriority(priority, prefabType, prefabName);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Debug.Log("Unable to read DynamicAssetSorter config file");
                Debug.Log(ex.Message);
            }
        }

        public bool SetUIPriority(int priority, string prefabType, string prefabName)
        {
            try
            {
                // Workaround to find right prefab info
                PrefabInfo prefab = new PrefabInfo();
                switch (prefabType)
                {
                    case "Network":
                        prefab = PrefabCollection<NetInfo>.FindLoaded(prefabName);
                        break;
                    case "Building":
                        prefab = PrefabCollection<BuildingInfo>.FindLoaded(prefabName);
                        break;
                    default:
                        Debug.Log($"Dynamic Asset Sorter - Could not convert prefab type {prefabType} for Prefab {prefabName}!");
                        return false;
                }

                // Set Priority
                if (prefab != null)
                {
                    prefab.m_UIPriority = priority;
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.Log("Dynamic Asset Sorter - SetBuildingPriority() - " + ex.Message);
                return false;
            }
        }

        // Forces a UI Refresh by checking for milestones
        public void RefreshUI()
        {
            try
            {
                UnlockManager unlockManager = GameObject.FindObjectOfType<UnlockManager>();
                if (unlockManager != null)
                    unlockManager.MilestonesUpdated();
            }
            catch (Exception ex)
            {
                Debug.Log("Dynamic Asset Sorter - RefreshUI() - " + ex.Message);
            }
        }
    }
}
