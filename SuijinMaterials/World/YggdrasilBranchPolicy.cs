using System;

namespace SuijinMaterials.World
{
    public static class YggdrasilBranchPolicy
    {
        public static BranchPlacementDecision Decide(int worldSeed, long rootUserId, uint rootId, double chance)
        {
            ulong hash = Fnv1A(worldSeed, rootUserId, rootId);
            double roll = (hash & 0xFFFFFFUL) / 16777216d;
            float height = (float)(((hash >> 24) & 0xFFFFUL) / 65535d);
            float angle = (float)(((hash >> 40) & 0xFFFFUL) / 65535d * 360d);
            return new BranchPlacementDecision(roll < Math.Max(0d, Math.Min(1d, chance)), height, angle);
        }

        public static BranchPlacementDecision Initialize(IYggdrasilRootState state, int worldSeed, long rootUserId, uint rootId, double chance)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Initialized)
            {
                return new BranchPlacementDecision(state.Eligible, state.Height01, state.AngleDegrees);
            }

            BranchPlacementDecision decision = Decide(worldSeed, rootUserId, rootId, chance);
            state.Eligible = decision.Eligible;
            state.Height01 = decision.Height01;
            state.AngleDegrees = decision.AngleDegrees;
            state.Depleted = false;
            state.MarkInitialized();
            return decision;
        }

        public static BranchHarvestResult TryHarvest(IYggdrasilRootState state, BranchHarvestContext context, Action emitLoot)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (!context.Authoritative) return BranchHarvestResult.RejectedNotAuthoritative;
            if (state.Depleted) return BranchHarvestResult.RejectedDepleted;
            if (!context.IsChop) return BranchHarvestResult.RejectedNotChop;
            if (context.PhysicalDamage <= 0f) return BranchHarvestResult.RejectedNoDamage;
            if (context.ToolTier < context.MinimumToolTier) return BranchHarvestResult.RejectedToolTier;

            state.Depleted = true;
            emitLoot?.Invoke();
            return BranchHarvestResult.Harvested;
        }

        private static ulong Fnv1A(int worldSeed, long userId, uint objectId)
        {
            const ulong offset = 14695981039346656037UL;
            const ulong prime = 1099511628211UL;
            ulong hash = offset;
            unchecked
            {
                foreach (byte value in BitConverter.GetBytes(worldSeed)) { hash ^= value; hash *= prime; }
                foreach (byte value in BitConverter.GetBytes(userId)) { hash ^= value; hash *= prime; }
                foreach (byte value in BitConverter.GetBytes(objectId)) { hash ^= value; hash *= prime; }
            }
            return hash;
        }
    }
}
