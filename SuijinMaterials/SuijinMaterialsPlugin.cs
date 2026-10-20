using System.IO;
using BepInEx;
using Jotunn.Managers;
using Jotunn.Utils;
using SuijinMaterials.Acquisition;
using SuijinMaterials.Config;
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
        public const string PluginVersion = "0.3.0";

        private MaterialRegistry materialRegistry;
        private MaterialsConfig materialsConfig;
        private AcquisitionCoordinator acquisitionCoordinator;

        private void Awake()
        {
            string pluginDirectory = Path.GetDirectoryName(Info.Location);
            materialsConfig = MaterialsConfig.Bind(Config);
            materialRegistry = new MaterialRegistry(
                new JotunnMaterialRegistrationBackend(),
                pluginDirectory,
                message => Logger.LogInfo(message),
                message => Logger.LogError(message));

            MaterialLocalization.RegisterEnglish();
            PrefabManager.OnVanillaPrefabsAvailable += RegisterMaterials;
            SynchronizationManager.OnConfigurationSynchronized += OnConfigurationSynchronized;
            Logger.LogInfo($"{PluginName} {PluginVersion} initialized");
        }

        private void RegisterMaterials()
        {
            PrefabManager.OnVanillaPrefabsAvailable -= RegisterMaterials;

            RegistrationSummary summary = materialRegistry.RegisterAll(MaterialCatalog.All);
            Logger.LogInfo(
                $"Material registration complete: {summary.RegisteredCount} registered, " +
                $"{summary.FailedCount} failed.");

            if (summary.FailedCount == 0)
            {
                acquisitionCoordinator = new AcquisitionCoordinator(materialsConfig, PluginGuid, message => Logger.LogError(message));
                AcquisitionInstallationSummary acquisition = acquisitionCoordinator.Install();
                Logger.LogInfo($"Acquisition installation complete: {acquisition.InstalledCount} installed, {acquisition.DisabledCount} disabled, {acquisition.FailedCount} failed.");
            }
            else
            {
                Logger.LogError("Acquisition rules were not installed because one or more material prefabs failed registration.");
            }
        }

        private void OnConfigurationSynchronized(object sender, ConfigurationSynchronizationEventArgs args)
        {
            if (acquisitionCoordinator == null) return;
            AcquisitionInstallationSummary summary = acquisitionCoordinator.Refresh();
            Logger.LogInfo($"Acquisition configuration refreshed: {summary.InstalledCount} active, {summary.DisabledCount} disabled.");
        }

        private void OnDestroy()
        {
            PrefabManager.OnVanillaPrefabsAvailable -= RegisterMaterials;
            SynchronizationManager.OnConfigurationSynchronized -= OnConfigurationSynchronized;
            acquisitionCoordinator?.Uninstall();
        }
    }
}
