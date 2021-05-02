using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace DynamicAssetSorter
{
    public static class PrefabInfoExtension
    {
        // Use reflection to get the category of prefabs
        public static string GetUICategory(PrefabInfo prefab)
        {
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            FieldInfo field = typeof(PrefabInfo).GetField("m_UICategory", flags);
            return (string)field.GetValue(prefab);
        }
    }
}
