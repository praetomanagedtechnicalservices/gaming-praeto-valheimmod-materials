namespace SuijinMaterials.World
{
    public sealed class WorldInstallationSummary
    {
        public WorldInstallationSummary(int installedRuleCount, int failedSystemCount)
        {
            InstalledRuleCount = installedRuleCount;
            FailedSystemCount = failedSystemCount;
        }

        public int InstalledRuleCount { get; }
        public int FailedSystemCount { get; }
    }
}
