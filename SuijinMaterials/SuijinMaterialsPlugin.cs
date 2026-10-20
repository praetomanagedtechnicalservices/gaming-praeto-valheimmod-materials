using System.IO;
using BepInEx;
using Jotunn.Managers;
using Jotunn.Utils;
using SuijinMaterials.Content;

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

        private MaterialRegistry materialRegistry;

        private void Awake()
        {
            string pluginDirectory = Path.GetDirectoryName(Info.Location);
            materialRegistry = new MaterialRegistry(
                new JotunnMaterialRegistrationBackend(),
                pluginDirectory,
                message => Logger.LogInfo(message),
                message => Logger.LogError(message));

            MaterialLocalization.RegisterEnglish();
            PrefabManager.OnVanillaPrefabsAvailable += RegisterMaterials;
            Logger.LogInfo($"{PluginName} {PluginVersion} initialized");
        }

        private void RegisterMaterials()
        {
            PrefabManager.OnVanillaPrefabsAvailable -= RegisterMaterials;

            RegistrationSummary summary = materialRegistry.RegisterAll(MaterialCatalog.All);
            Logger.LogInfo(
                $"Material registration complete: {summary.RegisteredCount} registered, " +
                $"{summary.FailedCount} failed.");
        }

        private void OnDestroy()
        {
            PrefabManager.OnVanillaPrefabsAvailable -= RegisterMaterials;
        }
    }
}
