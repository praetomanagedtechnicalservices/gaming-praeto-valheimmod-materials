using System.IO;
using BepInEx;
using Jotunn.Managers;
using Jotunn.Utils;
using SuijinMaterials.Acquisition;
using SuijinMaterials.Config;
using SuijinMaterials.Content;
using SuijinMaterials.Merchants;
using SuijinMaterials.World;

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
        private WorldMaterialsConfig worldMaterialsConfig;
        private MerchantStockConfig merchantStockConfig;
        private AcquisitionCoordinator acquisitionCoordinator;
        private MerchantStockCoordinator merchantStockCoordinator;
        private WorldMaterialCoordinator worldMaterialCoordinator;
        private bool worldInstalled;

        private void Awake()
        {
            string pluginDirectory = Path.GetDirectoryName(Info.Location);
            materialsConfig = MaterialsConfig.Bind(Config);
            worldMaterialsConfig = WorldMaterialsConfig.Bind(Config);
            merchantStockConfig = MerchantStockConfig.Bind(Config);
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
            Logger.LogInfo($"Material registration complete: {summary.RegisteredCount} registered, {summary.FailedCount} failed.");

            if (summary.FailedCount != 0)
            {
                Logger.LogError("Acquisition and v0.3 world systems were not installed because one or more material prefabs failed registration.");
                return;
            }

            acquisitionCoordinator = new AcquisitionCoordinator(materialsConfig, PluginGuid, message => Logger.LogError(message));
            AcquisitionInstallationSummary acquisition = acquisitionCoordinator.Install();
            Logger.LogInfo($"Acquisition installation complete: {acquisition.InstalledCount} installed, {acquisition.DisabledCount} disabled, {acquisition.FailedCount} failed.");

            merchantStockCoordinator = new MerchantStockCoordinator(merchantStockConfig, message => Logger.LogError(message));
            MerchantInstallationSummary merchants = merchantStockCoordinator.Install();
            Logger.LogInfo($"Merchant stock installation complete: {merchants.InstalledCount} installed, {merchants.DisabledCount} disabled, {merchants.FailedCount} failed.");

            worldMaterialCoordinator = new WorldMaterialCoordinator(worldMaterialsConfig, message => Logger.LogError(message));
            ZoneManager.OnVanillaVegetationAvailable += InstallWorldMaterials;
        }

        private void InstallWorldMaterials()
        {
            ZoneManager.OnVanillaVegetationAvailable -= InstallWorldMaterials;
            WorldInstallationSummary world = worldMaterialCoordinator.Install();
            worldInstalled = true;
            Logger.LogInfo($"World material installation complete: {world.InstalledRuleCount} active rules, {world.FailedSystemCount} failed systems.");
        }

        private void OnConfigurationSynchronized(object sender, ConfigurationSynchronizationEventArgs args)
        {
            if (acquisitionCoordinator != null)
            {
                AcquisitionInstallationSummary acquisition = acquisitionCoordinator.Refresh();
                Logger.LogInfo($"Acquisition configuration refreshed: {acquisition.InstalledCount} active, {acquisition.DisabledCount} disabled.");
            }
            if (merchantStockCoordinator != null)
            {
                MerchantInstallationSummary merchants = merchantStockCoordinator.Refresh();
                Logger.LogInfo($"Merchant stock configuration refreshed: {merchants.InstalledCount} active, {merchants.DisabledCount} disabled.");
            }
            if (worldInstalled && worldMaterialCoordinator != null)
            {
                WorldInstallationSummary world = worldMaterialCoordinator.Refresh();
                Logger.LogInfo($"World material configuration refreshed: {world.InstalledRuleCount} active rules, {world.FailedSystemCount} failed systems.");
            }
        }

        private void OnDestroy()
        {
            PrefabManager.OnVanillaPrefabsAvailable -= RegisterMaterials;
            ZoneManager.OnVanillaVegetationAvailable -= InstallWorldMaterials;
            SynchronizationManager.OnConfigurationSynchronized -= OnConfigurationSynchronized;
            worldMaterialCoordinator?.Uninstall();
            merchantStockCoordinator?.Uninstall();
            acquisitionCoordinator?.Uninstall();
        }
    }
}
