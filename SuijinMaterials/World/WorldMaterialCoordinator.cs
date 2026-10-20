using System;
using SuijinMaterials.Config;

namespace SuijinMaterials.World
{
    public sealed class WorldMaterialCoordinator
    {
        private readonly Func<WorldSettingsSnapshot> snapshot;
        private readonly Action<WorldSettingsSnapshot> installVegetation;
        private readonly Action uninstallVegetation;
        private readonly Action<WorldSettingsSnapshot> installBranches;
        private readonly Action uninstallBranches;
        private readonly Action<string> logError;

        public WorldMaterialCoordinator(WorldMaterialsConfig config, Action<string> logError)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            this.logError = logError ?? (_ => { });
            VegetationInstaller vegetation = new VegetationInstaller(new JotunnVegetationFacade(), this.logError);
            YggdrasilBranchInstaller branches = new YggdrasilBranchInstaller(this.logError);
            snapshot = config.Snapshot;
            installVegetation = vegetation.Install;
            uninstallVegetation = vegetation.Uninstall;
            installBranches = branches.Install;
            uninstallBranches = branches.Uninstall;
        }

        public WorldMaterialCoordinator(
            Func<WorldSettingsSnapshot> snapshot,
            Action<WorldSettingsSnapshot> installVegetation,
            Action uninstallVegetation,
            Action<WorldSettingsSnapshot> installBranches,
            Action uninstallBranches,
            Action<string> logError)
        {
            this.snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
            this.installVegetation = installVegetation ?? throw new ArgumentNullException(nameof(installVegetation));
            this.uninstallVegetation = uninstallVegetation ?? throw new ArgumentNullException(nameof(uninstallVegetation));
            this.installBranches = installBranches ?? throw new ArgumentNullException(nameof(installBranches));
            this.uninstallBranches = uninstallBranches ?? throw new ArgumentNullException(nameof(uninstallBranches));
            this.logError = logError ?? (_ => { });
        }

        public WorldInstallationSummary Install()
        {
            WorldSettingsSnapshot current = snapshot();
            int installed = 0;
            int failed = 0;
            try { installVegetation(current); installed += 2; }
            catch (Exception exception) { failed++; logError("Vegetation installation failed: " + exception.Message); }
            try { installBranches(current); installed++; }
            catch (Exception exception) { failed++; logError("Yggdrasil branch installation failed: " + exception.Message); }
            return new WorldInstallationSummary(installed, failed);
        }

        public WorldInstallationSummary Refresh() => Install();

        public void Uninstall()
        {
            try { uninstallBranches(); } catch (Exception exception) { logError("Yggdrasil branch teardown failed: " + exception.Message); }
            try { uninstallVegetation(); } catch (Exception exception) { logError("Vegetation teardown failed: " + exception.Message); }
        }
    }
}
