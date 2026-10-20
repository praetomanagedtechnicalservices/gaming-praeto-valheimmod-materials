using System.Linq;
using NUnit.Framework;
using SuijinMaterials.World;

namespace SuijinMaterials.Tests
{
    [TestFixture]
    public sealed class WorldMaterialCatalogTests
    {
        [Test]
        public void Catalog_has_three_approved_world_rules()
        {
            Assert.That(WorldMaterialCatalog.All.Select(x => x.RuleId), Is.EqualTo(new[] { "yarrow-meadows", "nightbloom-swamp", "yggdrasil-branch-roots" }));
            WorldMaterialDefinition yarrow = WorldMaterialCatalog.All[0];
            WorldMaterialDefinition branch = WorldMaterialCatalog.All[2];
            Assert.Multiple(() =>
            {
                Assert.That(yarrow.DefaultChance, Is.EqualTo(0.35));
                Assert.That(yarrow.DefaultGroupMin, Is.EqualTo(1));
                Assert.That(yarrow.DefaultGroupMax, Is.EqualTo(2));
                Assert.That(yarrow.DefaultRespawnMinutes, Is.EqualTo(240));
                Assert.That(branch.DefaultChance, Is.EqualTo(0.35));
                Assert.That(branch.Renewable, Is.False);
                Assert.That(branch.SourcePrefabIds, Is.Not.Empty);
            });
            Assert.That(WorldMaterialCatalogValidator.Validate(WorldMaterialCatalog.All), Is.Empty);
        }
    }
}
