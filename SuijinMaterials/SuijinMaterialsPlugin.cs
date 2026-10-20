using BepInEx;
using Jotunn.Utils;

namespace SuijinMaterials
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal sealed class SuijinMaterialsPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "dev.praeto.valheim.materials";
        public const string PluginName = "SuijinMaterials";
        public const string PluginVersion = "0.1.0";

        private void Awake()
        {
            Jotunn.Logger.LogInfo($"{PluginName} {PluginVersion} loaded");
        }
    }
}
