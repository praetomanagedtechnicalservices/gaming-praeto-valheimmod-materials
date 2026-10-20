namespace SuijinMaterials.World
{
    public sealed class BranchHarvestContext
    {
        public BranchHarvestContext(bool authoritative, bool isChop, float physicalDamage, int toolTier, int minimumToolTier)
        {
            Authoritative = authoritative;
            IsChop = isChop;
            PhysicalDamage = physicalDamage;
            ToolTier = toolTier;
            MinimumToolTier = minimumToolTier;
        }

        public bool Authoritative { get; }
        public bool IsChop { get; }
        public float PhysicalDamage { get; }
        public int ToolTier { get; }
        public int MinimumToolTier { get; }
    }
}
