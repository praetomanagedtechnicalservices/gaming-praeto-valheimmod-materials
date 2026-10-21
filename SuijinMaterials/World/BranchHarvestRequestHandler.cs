using System;

namespace SuijinMaterials.World
{
    public static class BranchHarvestRequestHandler
    {
        private const int AxesSkill = 7;
        private const int WoodCuttingSkill = 13;

        public static BranchHarvestResult Handle(
            bool authoritative,
            IYggdrasilRootState state,
            BranchHarvestRequest request,
            int minimumToolTier,
            Action emitLoot)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (!state.Initialized || !state.Eligible) return BranchHarvestResult.RejectedIneligible;

            bool isChop = request.Skill == AxesSkill || request.Skill == WoodCuttingSkill;
            BranchHarvestContext context = new BranchHarvestContext(
                authoritative,
                isChop,
                request.PhysicalDamage,
                request.ToolTier,
                minimumToolTier);
            return YggdrasilBranchPolicy.TryHarvest(state, context, emitLoot);
        }
    }
}
