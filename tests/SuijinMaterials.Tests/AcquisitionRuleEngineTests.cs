using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SuijinMaterials.Acquisition;

namespace SuijinMaterials.Tests
{
    [TestFixture]
    public sealed class AcquisitionRuleEngineTests
    {
        [Test]
        public void Elder_and_nest_rules_use_approved_probability_and_amounts()
        {
            Assert.That(Apply("gd_king", AcquisitionEventKind.CreatureDrops, 0.99).Single().Amount, Is.EqualTo(4));
            Assert.That(Apply("Spawner_GreydwarfNest(Clone)", AcquisitionEventKind.DestructibleDestroyed, 0.24), Has.Some.Property("PrefabId").EqualTo("praeto_mat_elderroot"));
            Assert.That(Apply("not_Spawner_GreydwarfNest", AcquisitionEventKind.DestructibleDestroyed, 0), Is.Empty);
        }

        [Test]
        public void Hen_conversion_conserves_total_and_does_not_mutate_input()
        {
            var input = new[] { new DropStack("Feathers", 2), new DropStack("Wood", 1), new DropStack("Feathers", 3) };
            IReadOnlyList<DropStack> result = Engine(0).Apply(new AcquisitionContext(AcquisitionEventKind.CreatureDrops, "Hen"), input);
            Assert.Multiple(() =>
            {
                Assert.That(result.Count(drop => drop.PrefabId == "praeto_mat_hen_feather"), Is.EqualTo(1));
                Assert.That(result.Single(drop => drop.PrefabId == "praeto_mat_hen_feather").Amount, Is.EqualTo(5));
                Assert.That(input.Count(drop => drop.PrefabId == "Feathers"), Is.EqualTo(2));
            });
        }

        [Test]
        public void Replacement_rules_are_one_for_one_and_location_scoped()
        {
            var crystal = new[] { new DropStack("Crystal", 6) };
            Assert.That(Engine(0).Apply(new AcquisitionContext(AcquisitionEventKind.CreatureDrops, "StoneGolem"), crystal).Single().PrefabId, Is.EqualTo("praeto_mat_silver_crystal"));
            var copper = new[] { new DropStack("CopperScrap", 3) };
            Assert.That(Engine(0).Apply(new AcquisitionContext(AcquisitionEventKind.ContainerLootGenerated, "dvergrprops_lantern", "Mistlands_Excavation1", isNaturalContainer: true), copper).Single().PrefabId, Is.EqualTo("praeto_mat_gold_scrap"));
            Assert.That(Engine(0).Apply(new AcquisitionContext(AcquisitionEventKind.ContainerLootGenerated, "dvergrprops_lantern", "MeadowsVillage", isNaturalContainer: true), copper).Single().PrefabId, Is.EqualTo("CopperScrap"));
        }

        [Test]
        public void Authority_level_probability_and_container_gates_are_enforced()
        {
            Assert.That(Engine(0).Apply(new AcquisitionContext(AcquisitionEventKind.CreatureDrops, "Wolf", sourceLevel: 1), Array.Empty<DropStack>()), Is.Empty);
            Assert.That(Engine(0).Apply(new AcquisitionContext(AcquisitionEventKind.CreatureDrops, "Wolf", sourceLevel: 2), Array.Empty<DropStack>()), Has.Count.EqualTo(1));
            Assert.That(Engine(0).Apply(new AcquisitionContext(AcquisitionEventKind.CreatureDrops, "Goblin", isAuthoritative: false), Array.Empty<DropStack>()), Is.Empty);
            Assert.That(Engine(0.12).Apply(new AcquisitionContext(AcquisitionEventKind.ContainerLootGenerated, "TreasureChest_meadows", isNaturalContainer: true), Array.Empty<DropStack>()), Is.Empty);
            Assert.That(Engine(0.119).Apply(new AcquisitionContext(AcquisitionEventKind.ContainerLootGenerated, "TreasureChest_meadows", isNaturalContainer: true), Array.Empty<DropStack>()), Has.Count.EqualTo(1));
            Assert.That(Engine(0).Apply(new AcquisitionContext(AcquisitionEventKind.ContainerLootGenerated, "TreasureChest_meadows", isNaturalContainer: true, isDungeonOrInterior: true), Array.Empty<DropStack>()), Is.Empty);
        }

        private static IReadOnlyList<DropStack> Apply(string source, AcquisitionEventKind kind, double unit) => Engine(unit).Apply(new AcquisitionContext(kind, source), Array.Empty<DropStack>());

        private static AcquisitionRuleEngine Engine(double unit)
        {
            var settings = AcquisitionCatalog.All.ToDictionary(rule => rule.RuleId, rule => new RuleSettings(rule.Kind != AcquisitionRuleKind.Disabled, rule.DefaultChance, rule.DefaultMinAmount, rule.DefaultMaxAmount));
            return new AcquisitionRuleEngine(AcquisitionCatalog.All, new AcquisitionSettingsSnapshot(settings), new FakeRandom(unit));
        }

        private sealed class FakeRandom : IRandomSource
        {
            private readonly double unit;
            public FakeRandom(double unit) { this.unit = unit; }
            public double NextUnit() => unit;
            public int NextInclusive(int minimum, int maximum) => maximum;
        }
    }
}
