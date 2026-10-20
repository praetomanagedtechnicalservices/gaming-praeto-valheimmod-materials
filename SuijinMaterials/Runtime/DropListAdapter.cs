using System;
using System.Collections.Generic;
using Jotunn.Managers;
using SuijinMaterials.Acquisition;
using UnityEngine;

namespace SuijinMaterials.Runtime
{
    internal static class DropListAdapter
    {
        public static IReadOnlyList<DropStack> ToStacks(IReadOnlyList<KeyValuePair<GameObject, int>> drops)
        {
            var result = new List<DropStack>();
            if (drops != null) foreach (KeyValuePair<GameObject, int> drop in drops) if (drop.Key != null && drop.Value > 0) result.Add(new DropStack(PrefabIdentity.Normalize(drop.Key.name), drop.Value));
            return result.AsReadOnly();
        }

        public static List<KeyValuePair<GameObject, int>> ToGameDrops(IReadOnlyList<DropStack> drops)
        {
            var result = new List<KeyValuePair<GameObject, int>>();
            foreach (DropStack drop in drops)
            {
                GameObject prefab = PrefabManager.Instance.GetPrefab(drop.PrefabId);
                if (prefab == null) throw new InvalidOperationException("Missing registered material prefab: " + drop.PrefabId);
                result.Add(new KeyValuePair<GameObject, int>(prefab, drop.Amount));
            }
            return result;
        }
    }
}
