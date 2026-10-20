using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SuijinMaterials.Acquisition;
using SuijinMaterials.Content;

namespace SuijinMaterials.Tests
{
    [TestFixture]
    public sealed class AcquisitionCatalogTests
    {
        [Test]
        public void Catalog_contains_every_v02_rule_once()
        {
            string[] expectedRuleIds =
            {
                "huginn-overworld-chest", "hen-feather-conversion", "elderroot-greydwarf-nest",
                "elderroot-elder", "rotten-elderroot-abomination", "runecarved-bone-skeleton-pile",
                "silver-crystal-stone-golem", "frostbitten-fang-fenring", "frostbitten-fang-wolf-one-star",
                "frostbitten-fang-wolf-two-star", "fuling-flesh", "fuling-hex-totem",
                "gold-scrap-mistlands-ruin", "dvergr-flesh", "seeker-venom-gland-seeker",
                "seeker-venom-gland-soldier", "gull-feather-disabled", "crow-feather-disabled",
                "ash-crow-feather-disabled", "munin-feather-disabled"
            };

            Assert.That(AcquisitionCatalog.All.Select(rule => rule.RuleId), Is.EqualTo(expectedRuleIds));
            Assert.That(AcquisitionCatalog.All.Select(rule => rule.RuleId), Is.Unique);
            Assert.That(
                AcquisitionCatalog.All.Select(rule => rule.MaterialPrefabId),
                Is.All.Matches<string>(id => MaterialCatalog.All.Any(material => material.PrefabId == id)));
        }

        [Test]
        public void Catalog_uses_the_approved_defaults_and_sources()
        {
            AssertRule("huginn-overworld-chest", AcquisitionRuleKind.AddContainerLoot, 0.12, 1, 1);
            AssertRule("elderroot-greydwarf-nest", AcquisitionRuleKind.AddedDestructibleDrop, 0.25, 1, 1);
            AssertRule("elderroot-elder", AcquisitionRuleKind.AddedBossDrop, 1.0, 1, 4);
            AssertRule("rotten-elderroot-abomination", AcquisitionRuleKind.AddedCreatureDrop, 0.25, 1, 1);
            AssertRule("runecarved-bone-skeleton-pile", AcquisitionRuleKind.AddedDestructibleDrop, 1.0, 1, 1);
            AssertRule("silver-crystal-stone-golem", AcquisitionRuleKind.ReplaceCreatureDrop, 1.0, 1, 1);
            AssertRule("frostbitten-fang-fenring", AcquisitionRuleKind.AddedCreatureDrop, 0.20, 1, 1);
            AssertRule("frostbitten-fang-wolf-one-star", AcquisitionRuleKind.AddedCreatureDrop, 0.15, 1, 1, 2);
            AssertRule("frostbitten-fang-wolf-two-star", AcquisitionRuleKind.AddedCreatureDrop, 0.30, 1, 1, 3);
            AssertRule("fuling-flesh", AcquisitionRuleKind.AddedCreatureDrop, 0.20, 1, 1);
            AssertRule("fuling-hex-totem", AcquisitionRuleKind.AddedCreatureDrop, 0.10, 1, 1);
            AssertRule("gold-scrap-mistlands-ruin", AcquisitionRuleKind.ReplaceContainerLoot, 1.0, 1, 1);
            AssertRule("dvergr-flesh", AcquisitionRuleKind.AddedCreatureDrop, 0.15, 1, 1);
            AssertRule("seeker-venom-gland-seeker", AcquisitionRuleKind.AddedCreatureDrop, 0.15, 1, 1);
            AssertRule("seeker-venom-gland-soldier", AcquisitionRuleKind.AddedCreatureDrop, 0.25, 1, 1);

            Assert.That(Rule("elderroot-greydwarf-nest").SourcePrefabIds, Does.Contain("Spawner_GreydwarfNest"));
            Assert.That(Rule("runecarved-bone-skeleton-pile").SourcePrefabIds, Is.SupersetOf(new[] { "BonePileSpawner", "BonePileSpawner_swamp" }));
            Assert.That(Rule("fuling-flesh").SourcePrefabIds, Is.EquivalentTo(new[] { "Goblin", "GoblinBrute", "GoblinShaman" }));
            Assert.That(Rule("fuling-hex-totem").SourcePrefabIds, Is.EqualTo(new[] { "GoblinShaman" }));
            Assert.That(Rule("silver-crystal-stone-golem").ReplacedPrefabId, Is.EqualTo("Crystal"));
            Assert.That(Rule("gold-scrap-mistlands-ruin").ReplacedPrefabId, Is.EqualTo("CopperScrap"));
            Assert.That(Rule("gold-scrap-mistlands-ruin").SourceLocationIds, Is.Not.Empty);
        }

        [Test]
        public void Raven_feather_rules_are_explicitly_disabled_without_sources()
        {
            string[] ids = { "gull-feather-disabled", "crow-feather-disabled", "ash-crow-feather-disabled", "munin-feather-disabled" };

            foreach (string id in ids)
            {
                AcquisitionDefinition rule = Rule(id);
                Assert.Multiple(() =>
                {
                    Assert.That(rule.Kind, Is.EqualTo(AcquisitionRuleKind.Disabled));
                    Assert.That(rule.SourcePrefabIds, Is.Empty);
                    Assert.That(rule.SourceLocationIds, Is.Empty);
                    Assert.That(rule.DisabledReason, Is.Not.Empty);
                });
            }
        }

        [Test]
        public void Validation_reports_invalid_definitions_independently()
        {
            var duplicate = Definition("duplicate", "praeto_mat_yarrow", 0.5, 1, 1, "Hen");
            var invalidChance = Definition("bad-chance", "praeto_mat_yarrow", 1.1, 1, 1, "Hen");
            var invalidStack = Definition("bad-stack", "praeto_mat_yarrow", 0.5, 3, 2, "Hen");
            var unknownMaterial = Definition("bad-material", "praeto_mat_unknown", 0.5, 1, 1, "Hen");
            var noSources = Definition("no-sources", "praeto_mat_yarrow", 0.5, 1, 1);

            IReadOnlyList<string> errors = AcquisitionCatalogValidator.Validate(new[]
            {
                duplicate, duplicate, invalidChance, invalidStack, unknownMaterial, noSources
            });

            Assert.Multiple(() =>
            {
                Assert.That(errors, Has.Some.Contains("Duplicate rule ID 'duplicate'"));
                Assert.That(errors, Has.Some.Contains("bad-chance").And.Contains("probability"));
                Assert.That(errors, Has.Some.Contains("bad-stack").And.Contains("amount"));
                Assert.That(errors, Has.Some.Contains("bad-material").And.Contains("unknown material"));
                Assert.That(errors, Has.Some.Contains("no-sources").And.Contains("source"));
            });
        }

        [Test]
        public void Settings_snapshot_is_immutable_and_requires_known_rule()
        {
            var source = new Dictionary<string, RuleSettings>
            {
                ["fuling-flesh"] = new RuleSettings(true, 0.2, 1, 1)
            };
            var snapshot = new AcquisitionSettingsSnapshot(source);
            source["fuling-flesh"] = new RuleSettings(false, 0, 1, 1);

            Assert.That(snapshot.Get("fuling-flesh").Enabled, Is.True);
            Assert.That(() => snapshot.Get("missing"), Throws.TypeOf<KeyNotFoundException>());
        }

        private static void AssertRule(
            string id,
            AcquisitionRuleKind kind,
            double chance,
            int minimum,
            int maximum,
            int sourceLevel = 0)
        {
            AcquisitionDefinition rule = Rule(id);
            Assert.Multiple(() =>
            {
                Assert.That(rule.Kind, Is.EqualTo(kind), id);
                Assert.That(rule.DefaultChance, Is.EqualTo(chance).Within(0.000001), id);
                Assert.That(rule.DefaultMinAmount, Is.EqualTo(minimum), id);
                Assert.That(rule.DefaultMaxAmount, Is.EqualTo(maximum), id);
                Assert.That(rule.MinimumSourceLevel, Is.EqualTo(sourceLevel), id);
            });
        }

        private static AcquisitionDefinition Rule(string id)
        {
            return AcquisitionCatalog.All.Single(rule => rule.RuleId == id);
        }

        private static AcquisitionDefinition Definition(
            string ruleId,
            string materialId,
            double chance,
            int minimum,
            int maximum,
            params string[] sources)
        {
            return new AcquisitionDefinition(
                ruleId,
                AcquisitionRuleKind.AddedCreatureDrop,
                materialId,
                sources,
                Array.Empty<string>(),
                chance,
                minimum,
                maximum);
        }
    }
}
