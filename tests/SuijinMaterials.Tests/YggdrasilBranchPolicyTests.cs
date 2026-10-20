using System;
using NUnit.Framework;
using SuijinMaterials.World;

namespace SuijinMaterials.Tests
{
    [TestFixture]
    public sealed class YggdrasilBranchPolicyTests
    {
        [Test]
        public void Placement_is_deterministic_and_honors_probability_edges()
        {
            BranchPlacementDecision first = YggdrasilBranchPolicy.Decide(123, 456, 789, .35);
            BranchPlacementDecision again = YggdrasilBranchPolicy.Decide(123, 456, 789, .35);
            Assert.Multiple(() =>
            {
                Assert.That(again.Eligible, Is.EqualTo(first.Eligible));
                Assert.That(again.Height01, Is.EqualTo(first.Height01));
                Assert.That(again.AngleDegrees, Is.EqualTo(first.AngleDegrees));
                Assert.That(YggdrasilBranchPolicy.Decide(1, 2, 3, 0).Eligible, Is.False);
                Assert.That(YggdrasilBranchPolicy.Decide(1, 2, 3, 1).Eligible, Is.True);
            });
        }

        [Test]
        public void Reload_uses_saved_decision_without_reinitializing()
        {
            FakeState state = new FakeState();
            BranchPlacementDecision first = YggdrasilBranchPolicy.Initialize(state, 1, 2, 3, 1);
            state.Depleted = true;
            BranchPlacementDecision reload = YggdrasilBranchPolicy.Initialize(state, 99, 88, 77, 0);
            Assert.Multiple(() =>
            {
                Assert.That(reload.Eligible, Is.EqualTo(first.Eligible));
                Assert.That(state.MarkCount, Is.EqualTo(1));
                Assert.That(state.Depleted, Is.True);
            });
        }

        [TestCase(false, true, 1, 0, BranchHarvestResult.RejectedNotAuthoritative)]
        [TestCase(true, false, 1, 0, BranchHarvestResult.RejectedNotChop)]
        [TestCase(true, true, 0, 0, BranchHarvestResult.RejectedNoDamage)]
        [TestCase(true, true, 1, 0, BranchHarvestResult.RejectedToolTier)]
        public void Harvest_rejects_invalid_hits(bool owner, bool chop, float damage, int tier, BranchHarvestResult expected)
        {
            FakeState state = new FakeState();
            int emissions = 0;
            BranchHarvestResult result = YggdrasilBranchPolicy.TryHarvest(state, new BranchHarvestContext(owner, chop, damage, tier, 1), () => emissions++);
            Assert.Multiple(() => { Assert.That(result, Is.EqualTo(expected)); Assert.That(state.Depleted, Is.False); Assert.That(emissions, Is.Zero); });
        }

        [Test]
        public void Harvest_persists_depletion_before_exactly_one_emission()
        {
            FakeState state = new FakeState();
            int emissions = 0;
            Action emit = () => { Assert.That(state.Depleted, Is.True); emissions++; };
            Assert.That(YggdrasilBranchPolicy.TryHarvest(state, new BranchHarvestContext(true, true, 1, 0, 0), emit), Is.EqualTo(BranchHarvestResult.Harvested));
            Assert.That(YggdrasilBranchPolicy.TryHarvest(state, new BranchHarvestContext(true, true, 1, 0, 0), emit), Is.EqualTo(BranchHarvestResult.RejectedDepleted));
            Assert.That(emissions, Is.EqualTo(1));
        }

        private sealed class FakeState : IYggdrasilRootState
        {
            public bool Initialized { get; private set; }
            public bool Eligible { get; set; }
            public bool Depleted { get; set; }
            public float Height01 { get; set; }
            public float AngleDegrees { get; set; }
            public int MarkCount { get; private set; }
            public void MarkInitialized() { Initialized = true; MarkCount++; }
        }
    }
}
