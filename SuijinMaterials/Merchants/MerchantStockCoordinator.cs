using System;
using SuijinMaterials.Config;

namespace SuijinMaterials.Merchants
{
    public sealed class MerchantStockCoordinator
    {
        private readonly Func<MerchantStockSettings> snapshot;
        private readonly MerchantStockInstaller installer;

        public MerchantStockCoordinator(MerchantStockConfig config, Action<string> logError)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            snapshot = config.Snapshot;
            installer = new MerchantStockInstaller(new JotunnMerchantStockFacade(), logError);
        }

        public MerchantStockCoordinator(Func<MerchantStockSettings> snapshot, MerchantStockInstaller installer)
        {
            this.snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
            this.installer = installer ?? throw new ArgumentNullException(nameof(installer));
        }

        public MerchantInstallationSummary Install() => installer.Install(snapshot());
        public MerchantInstallationSummary Refresh() => installer.Install(snapshot());
        public void Uninstall() => installer.Uninstall();
    }
}
