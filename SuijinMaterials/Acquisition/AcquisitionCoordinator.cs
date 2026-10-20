using System;
using HarmonyLib;
using SuijinMaterials.Config;
using SuijinMaterials.Runtime;

namespace SuijinMaterials.Acquisition
{
    public sealed class AcquisitionCoordinator
    {
        private readonly MaterialsConfig config;
        private readonly CreatureDropInstaller installer;
        private readonly Harmony harmony;
        private readonly Action<string> logError;
        private bool installed;

        public AcquisitionCoordinator(MaterialsConfig config, string harmonyId, Action<string> logError)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.logError = logError ?? (_ => { });
            installer = new CreatureDropInstaller(settings => AcquisitionRuntime.Configure(settings, this.logError));
            harmony = new Harmony(harmonyId ?? throw new ArgumentNullException(nameof(harmonyId)));
        }

        public AcquisitionInstallationSummary Install()
        {
            if (!installed)
            {
                harmony.PatchAll(typeof(AcquisitionCoordinator).Assembly);
                installed = true;
            }
            return Convert(installer.Install(config.Snapshot()));
        }

        public AcquisitionInstallationSummary Refresh()
        {
            if (!installed) return new AcquisitionInstallationSummary(0, 0, 0);
            return Convert(installer.Install(config.Snapshot()));
        }

        public void Uninstall()
        {
            installer.Uninstall();
            harmony.UnpatchSelf();
            installed = false;
        }

        private static AcquisitionInstallationSummary Convert(InstallationSummary summary) => new AcquisitionInstallationSummary(summary.InstalledCount, summary.DisabledCount, summary.FailedCount);
    }
}
