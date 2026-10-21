using System;
using NUnit.Framework;
using SuijinMaterials.World;

namespace SuijinMaterials.Tests
{
    [TestFixture]
    public sealed class BranchHarvestRequestHandlerTests
    {
        [Test]
        public void Non_owner_is_rejected_without_state_or_loot()
        {
            FakeState state = EligibleState();
            int emissions = 0;

            BranchHarvestResult result = BranchHarvestRequestHandler.Handle(
                false, state, Request(7, 10f, 1), 0, () => emissions++);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(BranchHarvestResult.RejectedNotAuthoritative));
                Assert.That(state.Depleted, Is.False);
                Assert.That(emissions, Is.Zero);
            });
        }

        [Test]
        public void Ineligible_initialized_root_is_rejected()
        {
            FakeState state = new FakeState { InitializedValue = true, Eligible = false };
            int emissions = 0;

            BranchHarvestResult result = BranchHarvestRequestHandler.Handle(
                true, state, Request(7, 10f, 1), 0, () => emissions++);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(BranchHarvestResult.RejectedIneligible));
                Assert.That(state.Depleted, Is.False);
                Assert.That(emissions, Is.Zero);
            });
        }

        [TestCase(7)]
        [TestCase(13)]
        public void Axes_and_woodcutting_are_accepted(int skill)
        {
            FakeState state = EligibleState();
            int emissions = 0;

            BranchHarvestResult result = BranchHarvestRequestHandler.Handle(
                true, state, Request(skill, 10f, 0), 0,
                () =>
                {
                    Assert.That(state.Depleted, Is.True);
                    emissions++;
                });

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(BranchHarvestResult.Harvested));
                Assert.That(emissions, Is.EqualTo(1));
            });
        }

        [TestCase(1, 10f, 1, 0, BranchHarvestResult.RejectedNotChop)]
        [TestCase(7, 0f, 1, 0, BranchHarvestResult.RejectedNoDamage)]
        [TestCase(7, 10f, 0, 1, BranchHarvestResult.RejectedToolTier)]
        public void Non_chop_zero_damage_and_low_tier_are_rejected(
            int skill, float damage, int toolTier, int minimumToolTier, BranchHarvestResult expected)
        {
            FakeState state = EligibleState();
            int emissions = 0;

            BranchHarvestResult result = BranchHarvestRequestHandler.Handle(
                true, state, Request(skill, damage, toolTier), minimumToolTier, () => emissions++);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(expected));
                Assert.That(state.Depleted, Is.False);
                Assert.That(emissions, Is.Zero);
            });
        }

        [Test]
        public void Duplicate_request_after_depletion_emits_once()
        {
            FakeState state = EligibleState();
            int emissions = 0;
            BranchHarvestRequest request = Request(7, 10f, 0);

            BranchHarvestResult first = BranchHarvestRequestHandler.Handle(true, state, request, 0, () => emissions++);
            BranchHarvestResult duplicate = BranchHarvestRequestHandler.Handle(true, state, request, 0, () => emissions++);

            Assert.Multiple(() =>
            {
                Assert.That(first, Is.EqualTo(BranchHarvestResult.Harvested));
                Assert.That(duplicate, Is.EqualTo(BranchHarvestResult.RejectedDepleted));
                Assert.That(emissions, Is.EqualTo(1));
            });
        }

        [TestCaseSource(nameof(NonFiniteDamageValues))]
        public void Non_finite_damage_is_rejected_without_state_or_loot(float damage)
        {
            FakeState state = EligibleState();
            int emissions = 0;

            BranchHarvestResult result = BranchHarvestRequestHandler.Handle(
                true, state, Request(7, damage, 0), 0, () => emissions++);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(BranchHarvestResult.RejectedNoDamage));
                Assert.That(state.Depleted, Is.False);
                Assert.That(emissions, Is.Zero);
            });
        }

        private static readonly float[] NonFiniteDamageValues =
        {
            float.NaN,
            float.PositiveInfinity,
            float.NegativeInfinity
        };

        private static BranchHarvestRequest Request(int skill, float damage, int tier) =>
            new BranchHarvestRequest(skill, damage, tier, 1f, 2f, 3f);

        private static FakeState EligibleState() =>
            new FakeState { InitializedValue = true, Eligible = true };

        private sealed class FakeState : IYggdrasilRootState
        {
            public bool InitializedValue { get; set; }
            public bool Initialized => InitializedValue;
            public bool Eligible { get; set; }
            public bool Depleted { get; set; }
            public float Height01 { get; set; }
            public float AngleDegrees { get; set; }
            public void MarkInitialized() => InitializedValue = true;
        }
    }
}
