using System;
using System.Collections.Generic;
using HarmonyLib;
using Jotunn.Managers;
using SuijinMaterials.Acquisition;
using SuijinMaterials.Runtime;
using UnityEngine;

namespace SuijinMaterials.Patches
{
    internal static class DestructionDropEmitter
    {
        public static void Emit(Component instance)
        {
            if (instance == null || !RuntimeAuthority.IsAuthoritative(instance)) return;
            string source = PrefabIdentity.Normalize(instance.gameObject.name);
            string key = "destroy:" + instance.gameObject.GetInstanceID();
            if (!AcquisitionRuntime.TryBegin(key)) return;
            var context = new AcquisitionContext(AcquisitionEventKind.DestructibleDestroyed, source, isAuthoritative: true);
            IReadOnlyList<DropStack> drops = AcquisitionRuntime.Apply(context, Array.Empty<DropStack>());
            foreach (DropStack drop in drops)
            {
                GameObject prefab = PrefabManager.Instance.GetPrefab(drop.PrefabId);
                if (prefab == null) continue;
                GameObject item = UnityEngine.Object.Instantiate(prefab, instance.transform.position + Vector3.up * 0.5f, Quaternion.identity);
                ItemDrop itemDrop = item.GetComponent<ItemDrop>();
                if (itemDrop != null) itemDrop.m_itemData.m_stack = drop.Amount;
            }
        }
    }

    [HarmonyPatch(typeof(Destructible), nameof(Destructible.Destroy))]
    internal static class DestructiblePatches
    {
        private static void Prefix(Destructible __instance) { try { DestructionDropEmitter.Emit(__instance); } catch (Exception e) { AcquisitionRuntime.Error(e, "Destructible"); } }
    }

    [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.Destroy))]
    internal static class WearNTearPatches
    {
        private static void Prefix(WearNTear __instance) { try { DestructionDropEmitter.Emit(__instance); } catch (Exception e) { AcquisitionRuntime.Error(e, "WearNTear"); } }
    }
}
