namespace SuijinMaterials.World
{
    public sealed class BranchHarvestRequest
    {
        public BranchHarvestRequest(int skill, float physicalDamage, int toolTier, float hitX, float hitY, float hitZ)
        {
            Skill = skill;
            PhysicalDamage = physicalDamage;
            ToolTier = toolTier;
            HitX = hitX;
            HitY = hitY;
            HitZ = hitZ;
        }

        public int Skill { get; }
        public float PhysicalDamage { get; }
        public int ToolTier { get; }
        public float HitX { get; }
        public float HitY { get; }
        public float HitZ { get; }
    }
}
