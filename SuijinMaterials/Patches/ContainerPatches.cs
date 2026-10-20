using System;
using HarmonyLib;
using Jotunn.Managers;
using SuijinMaterials.Acquisition;
using SuijinMaterials.Runtime;
using UnityEngine;

namespace SuijinMaterials.Patches
{
    [HarmonyPatch(typeof(Container), nameof(Container.AddDefaultItems))]
    internal static class ContainerPatches
    {
        private static void Prefix(Container __instance, out bool __state) { __state = __instance?.GetInventory()?.NrOfItems() > 0; }
        private static void Postfix(Container __instance, bool __state)
        {
            try
            {
                if (__instance == null) return;
                string source = PrefabIdentity.Normalize(__instance.gameObject.name);
                var facts = new ContainerFacts(source, RuntimeAuthority.IsAuthoritative(__instance), __instance.GetComponent<Piece>() == null, Location.GetLocation(__instance.transform.position, false)?.m_hasInterior == true, __instance.GetComponent<Piece>() != null, __state);
                if (!ContainerEligibility.Evaluate(facts)) return;
                string key = "container:" + __instance.gameObject.GetInstanceID();
                if (!AcquisitionRuntime.TryBegin(key)) return;
                var context = new AcquisitionContext(AcquisitionEventKind.ContainerLootGenerated, source, isAuthoritative: true, isNaturalContainer: true);
                foreach (DropStack drop in AcquisitionRuntime.Apply(context, Array.Empty<DropStack>()))
                {
                    GameObject prefab = PrefabManager.Instance.GetPrefab(drop.PrefabId);
                    ItemDrop item = prefab == null ? null : prefab.GetComponent<ItemDrop>();
                    if (item != null) __instance.GetInventory().AddItem(prefab, drop.Amount);
                }
            }
            catch (Exception exception) { AcquisitionRuntime.Error(exception, "Container"); }
        }
    }

    [HarmonyPatch(typeof(DropOnDestroyed), nameof(DropOnDestroyed.OnDestroyed))]
    internal static class MistlandsCopperReplacementPatch
    {
        private static void Prefix(DropOnDestroyed __instance)
        {
            try
            {
                if (__instance == null || !RuntimeAuthority.IsAuthoritative(__instance)) return;
                string source = PrefabIdentity.Normalize(__instance.gameObject.name);
                Location location = Location.GetLocation(__instance.transform.position, false);
                string locationId = location == null ? string.Empty : PrefabIdentity.Normalize(location.gameObject.name);
                AcquisitionDefinition rule = System.Linq.Enumerable.First(AcquisitionCatalog.All, item => item.RuleId == "gold-scrap-mistlands-ruin");
                if (!System.Linq.Enumerable.Any(rule.SourcePrefabIds, item => PrefabIdentity.Equals(item, source)) || !System.Linq.Enumerable.Any(rule.SourceLocationIds, item => PrefabIdentity.Equals(item, locationId))) return;
                GameObject copper = PrefabManager.Instance.GetPrefab(rule.ReplacedPrefabId);
                GameObject gold = PrefabManager.Instance.GetPrefab(rule.MaterialPrefabId);
                if (copper == null || gold == null) return;
                for (int index = 0; index < __instance.m_dropWhenDestroyed.m_drops.Count; index++)
                {
                    DropTable.DropData data = __instance.m_dropWhenDestroyed.m_drops[index];
                    if (data.m_item == copper || PrefabIdentity.Equals(data.m_item?.name, copper.name))
                    {
                        data.m_item = gold;
                        __instance.m_dropWhenDestroyed.m_drops[index] = data;
                    }
                }
            }
            catch (Exception exception) { AcquisitionRuntime.Error(exception, "Mistlands ruin"); }
        }
    }
}
