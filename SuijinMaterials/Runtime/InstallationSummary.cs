namespace SuijinMaterials.Runtime
{
    public sealed class InstallationSummary
    {
        public InstallationSummary(int installed, int disabled, int failed)
        {
            InstalledCount = installed;
            DisabledCount = disabled;
            FailedCount = failed;
        }
        public int InstalledCount { get; }
        public int DisabledCount { get; }
        public int FailedCount { get; }
    }
}
