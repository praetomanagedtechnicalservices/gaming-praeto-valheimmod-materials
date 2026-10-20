using HarmonyLib;
using SuijinMaterials.World;

namespace SuijinMaterials.Patches
{
    [HarmonyPatch(typeof(ZNetView), nameof(ZNetView.Awake))]
    internal static class YggdrasilRootAwakePatch
    {
        private static void Postfix(ZNetView __instance)
        {
            YggdrasilBranchInstaller.Active?.ObserveRoot(__instance);
        }
    }
}
