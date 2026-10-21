using System;

namespace SuijinMaterials.World
{
    public static class BranchHitAdapter
    {
        public static void Submit(
            IBranchHarvestRequestSink sink,
            int skill,
            float physicalDamage,
            int toolTier,
            float hitX,
            float hitY,
            float hitZ)
        {
            if (sink == null) throw new ArgumentNullException(nameof(sink));
            sink.RequestHarvest(new BranchHarvestRequest(skill, physicalDamage, toolTier, hitX, hitY, hitZ));
        }
    }
}
