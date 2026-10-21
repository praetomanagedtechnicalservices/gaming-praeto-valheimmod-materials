using System.IO;
using BepInEx;
using Jotunn.Managers;
using Jotunn.Utils;
using SuijinMaterials.Acquisition;
using SuijinMaterials.Config;
using SuijinMaterials.Content;
using SuijinMaterials.Merchants;
using SuijinMaterials.Runtime;
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
        public const string PluginVersion = "0.5.0";

        private MaterialRegistry materialRegistry;
        private MaterialsConfig materialsConfig;
        private WorldMaterialsConfig worldMaterialsConfig;
        private MerchantStockConfig merchantStockConfig;
        private AcquisitionCoordinator acquisitionCoordinator;
        private MerchantStockCoordinator merchantStockCoordinator;
        private WorldMaterialCoordinator worldMaterialCoordinator;
        private PluginLifecycleController lifecycle;

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

            lifecycle = new PluginLifecycleController(
                RegisterMaterials,
                new PluginSubsystem("acquisition", InstallAcquisition, RefreshAcquisition, UninstallAcquisition),
                new PluginSubsystem("merchants", InstallMerchants, RefreshMerchants, UninstallMerchants),
                new PluginSubsystem("world", InstallWorldMaterials, RefreshWorldMaterials, UninstallWorldMaterials),
                message => Logger.LogError(message));

            MaterialLocalization.RegisterEnglish();
            PrefabManager.OnVanillaPrefabsAvailable += OnVanillaPrefabsAvailable;
            ZoneManager.OnVanillaVegetationAvailable += OnVanillaVegetationAvailable;
            SynchronizationManager.OnConfigurationSynchronized += OnConfigurationSynchronized;
            Logger.LogInfo($"{PluginName} {PluginVersion} initialized");
        }

        private void OnVanillaPrefabsAvailable() => lifecycle?.OnVanillaPrefabsAvailable();
        private void OnVanillaVegetationAvailable() => lifecycle?.OnVanillaVegetationAvailable();
        private void OnConfigurationSynchronized(object sender, ConfigurationSynchronizationEventArgs args) => lifecycle?.OnConfigurationSynchronized();

        private bool RegisterMaterials()
        {
            RegistrationSummary summary = materialRegistry.RegisterAll(MaterialCatalog.All);
            Logger.LogInfo($"Material registration complete: {summary.RegisteredCount} registered, {summary.FailedCount} failed.");
            if (summary.FailedCount == 0) return true;
            Logger.LogError("Dependent runtime systems were not installed because one or more material prefabs failed registration.");
            return false;
        }

        private void InstallAcquisition()
        {
            acquisitionCoordinator = new AcquisitionCoordinator(materialsConfig, PluginGuid, message => Logger.LogError(message));
            AcquisitionInstallationSummary summary = acquisitionCoordinator.Install();
            Logger.LogInfo($"Acquisition installation complete: {summary.InstalledCount} installed, {summary.DisabledCount} disabled, {summary.FailedCount} failed.");
        }

        private void RefreshAcquisition()
        {
            AcquisitionInstallationSummary summary = acquisitionCoordinator.Refresh();
            Logger.LogInfo($"Acquisition configuration refreshed: {summary.InstalledCount} active, {summary.DisabledCount} disabled.");
        }

        private void UninstallAcquisition()
        {
            acquisitionCoordinator?.Uninstall();
            acquisitionCoordinator = null;
        }

        private void InstallMerchants()
        {
            merchantStockCoordinator = new MerchantStockCoordinator(merchantStockConfig, message => Logger.LogError(message));
            MerchantInstallationSummary summary = merchantStockCoordinator.Install();
            Logger.LogInfo($"Merchant stock installation complete: {summary.InstalledCount} installed, {summary.DisabledCount} disabled, {summary.FailedCount} failed.");
        }

        private void RefreshMerchants()
        {
            MerchantInstallationSummary summary = merchantStockCoordinator.Refresh();
            Logger.LogInfo($"Merchant stock configuration refreshed: {summary.InstalledCount} active, {summary.DisabledCount} disabled.");
        }

        private void UninstallMerchants()
        {
            merchantStockCoordinator?.Uninstall();
            merchantStockCoordinator = null;
        }

        private void InstallWorldMaterials()
        {
            worldMaterialCoordinator = new WorldMaterialCoordinator(worldMaterialsConfig, message => Logger.LogError(message));
            WorldInstallationSummary summary = worldMaterialCoordinator.Install();
            Logger.LogInfo($"World material installation complete: {summary.InstalledRuleCount} active rules, {summary.FailedSystemCount} failed systems.");
        }

        private void RefreshWorldMaterials()
        {
            WorldInstallationSummary summary = worldMaterialCoordinator.Refresh();
            Logger.LogInfo($"World material configuration refreshed: {summary.InstalledRuleCount} active rules, {summary.FailedSystemCount} failed systems.");
        }

        private void UninstallWorldMaterials()
        {
            worldMaterialCoordinator?.Uninstall();
            worldMaterialCoordinator = null;
        }

        private void OnDestroy()
        {
            PrefabManager.OnVanillaPrefabsAvailable -= OnVanillaPrefabsAvailable;
            ZoneManager.OnVanillaVegetationAvailable -= OnVanillaVegetationAvailable;
            SynchronizationManager.OnConfigurationSynchronized -= OnConfigurationSynchronized;
            lifecycle?.Teardown();
            lifecycle = null;
        }
    }
}
