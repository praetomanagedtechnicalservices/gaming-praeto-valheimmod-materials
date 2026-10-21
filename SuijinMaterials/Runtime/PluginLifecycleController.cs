using System;

namespace SuijinMaterials.Runtime
{
    public sealed class PluginLifecycleController
    {
        private readonly Func<bool> registerMaterials;
        private readonly PluginSubsystem acquisition;
        private readonly PluginSubsystem merchants;
        private readonly PluginSubsystem world;
        private readonly Action<string> logError;
        private bool registryAttempted;
        private bool registryReady;
        private bool vegetationAvailable;
        private bool worldAttempted;
        private bool acquisitionActive;
        private bool merchantsActive;
        private bool worldActive;
        private bool destroyed;

        public PluginLifecycleController(
            Func<bool> registerMaterials,
            PluginSubsystem acquisition,
            PluginSubsystem merchants,
            PluginSubsystem world,
            Action<string> logError)
        {
            this.registerMaterials = registerMaterials ?? throw new ArgumentNullException(nameof(registerMaterials));
            this.acquisition = acquisition ?? throw new ArgumentNullException(nameof(acquisition));
            this.merchants = merchants ?? throw new ArgumentNullException(nameof(merchants));
            this.world = world ?? throw new ArgumentNullException(nameof(world));
            this.logError = logError ?? (_ => { });
        }

        public void OnVanillaPrefabsAvailable()
        {
            if (destroyed || registryAttempted) return;
            registryAttempted = true;
            try
            {
                registryReady = registerMaterials();
            }
            catch (Exception exception)
            {
                logError("Material registry installation failed: " + exception.Message);
                return;
            }
            if (!registryReady) return;

            TryInstall(acquisition, ref acquisitionActive);
            TryInstall(merchants, ref merchantsActive);
            if (vegetationAvailable) TryInstallWorld();
        }

        public void OnVanillaVegetationAvailable()
        {
            if (destroyed) return;
            vegetationAvailable = true;
            if (registryReady) TryInstallWorld();
        }

        public void OnConfigurationSynchronized()
        {
            if (destroyed) return;
            TryRefresh(acquisition, acquisitionActive);
            TryRefresh(merchants, merchantsActive);
            TryRefresh(world, worldActive);
        }

        public void Teardown()
        {
            if (destroyed) return;
            destroyed = true;
            TryUninstall(world, ref worldActive);
            TryUninstall(merchants, ref merchantsActive);
            TryUninstall(acquisition, ref acquisitionActive);
        }

        private void TryInstallWorld()
        {
            if (worldAttempted) return;
            worldAttempted = true;
            TryInstall(world, ref worldActive);
        }

        private void TryInstall(PluginSubsystem subsystem, ref bool active)
        {
            if (active) return;
            try
            {
                subsystem.Install();
                active = true;
            }
            catch (Exception exception)
            {
                logError("Subsystem '" + subsystem.Name + "' installation failed: " + exception.Message);
                try { subsystem.Uninstall(); }
                catch (Exception rollback) { logError("Subsystem '" + subsystem.Name + "' rollback failed: " + rollback.Message); }
            }
        }

        private void TryRefresh(PluginSubsystem subsystem, bool active)
        {
            if (!active) return;
            try { subsystem.Refresh(); }
            catch (Exception exception) { logError("Subsystem '" + subsystem.Name + "' refresh failed: " + exception.Message); }
        }

        private void TryUninstall(PluginSubsystem subsystem, ref bool active)
        {
            if (!active) return;
            active = false;
            try { subsystem.Uninstall(); }
            catch (Exception exception) { logError("Subsystem '" + subsystem.Name + "' teardown failed: " + exception.Message); }
        }
    }
}
