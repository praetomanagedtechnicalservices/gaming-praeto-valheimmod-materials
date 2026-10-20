using System;
using System.Collections.Generic;
using HarmonyLib;
using SuijinMaterials.Acquisition;
using SuijinMaterials.Runtime;
using UnityEngine;

namespace SuijinMaterials.Patches
{
    [HarmonyPatch(typeof(CharacterDrop), nameof(CharacterDrop.GenerateDropList))]
    internal static class CharacterDropPatches
    {
        private static void Postfix(CharacterDrop __instance, ref List<KeyValuePair<GameObject, int>> __result)
        {
            try
            {
                if (__instance == null || __result == null) return;
                Character character = __instance.GetComponent<Character>();
                int level = character == null ? 0 : character.GetLevel();
                var context = new AcquisitionContext(AcquisitionEventKind.CreatureDrops, PrefabIdentity.Normalize(__instance.gameObject.name), sourceLevel: level, isAuthoritative: RuntimeAuthority.IsAuthoritative(__instance));
                __result = DropListAdapter.ToGameDrops(AcquisitionRuntime.Apply(context, DropListAdapter.ToStacks(__result)));
            }
            catch (Exception exception) { AcquisitionRuntime.Error(exception, "CharacterDrop"); }
        }
    }
}
