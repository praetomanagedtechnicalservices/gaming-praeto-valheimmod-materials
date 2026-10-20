namespace SuijinMaterials.Acquisition
{
    public sealed class AcquisitionInstallationSummary
    {
        public AcquisitionInstallationSummary(int installed, int disabled, int failed)
        { InstalledCount = installed; DisabledCount = disabled; FailedCount = failed; }
        public int InstalledCount { get; }
        public int DisabledCount { get; }
        public int FailedCount { get; }
    }
}
