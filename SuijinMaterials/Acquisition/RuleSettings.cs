namespace SuijinMaterials.Acquisition
{
    public sealed class RuleSettings
    {
        public RuleSettings(bool enabled, double chance, int minimumAmount, int maximumAmount)
        {
            Enabled = enabled;
            Chance = chance;
            MinimumAmount = minimumAmount;
            MaximumAmount = maximumAmount;
        }

        public bool Enabled { get; }
        public double Chance { get; }
        public int MinimumAmount { get; }
        public int MaximumAmount { get; }
    }
}
