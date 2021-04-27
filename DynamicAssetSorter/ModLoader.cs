using System;
using UnityEngine;
using ICities;

namespace DynamicAssetSorter
{
    public class ModLoader : LoadingExtensionBase
    {
        private static DynamicAssetSorter instance;

        public override void OnLevelLoaded(LoadMode mode)
        {
            try
            {
                // Don't run the mod inside asset editors
                if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame)
                    return;

                DynamicAssetSorter.isGameLoaded = true;

                if (instance != null)
                    GameObject.DestroyImmediate(instance.gameObject);

                instance = new GameObject("DynamicAssetSorter").AddComponent<DynamicAssetSorter>();
            }
            catch (Exception ex)
            {
                Debug.Log("Dynamic Asset Sorter - OnLevelLoaded() - " + ex.Message);
            }
        }

        // Destroy Mod when unloading
        public override void OnLevelUnloading()
        {
            if (instance != null)
                GameObject.DestroyImmediate(instance.gameObject);
        }
    }
}
