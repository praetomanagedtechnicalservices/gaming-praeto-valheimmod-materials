using System;
using System.Linq;
using NUnit.Framework;
using SuijinMaterials.Content;

namespace SuijinMaterials.Tests
{
    [TestFixture]
    public sealed class MaterialCatalogTests
    {
        private static readonly string[] ExpectedIds =
        {
            "praeto_mat_huginn_feather",
            "praeto_mat_gull_feather",
            "praeto_mat_crow_feather",
            "praeto_mat_ash_crow_feather",
            "praeto_mat_munin_feather",
            "praeto_mat_hen_feather",
            "praeto_mat_aged_tissue",
            "praeto_mat_voodoo_doll",
            "praeto_mat_canvas",
            "praeto_mat_yarrow",
            "praeto_mat_elderroot",
            "praeto_mat_nightbloom",
            "praeto_mat_rotten_elderroot",
            "praeto_mat_runecarved_bone",
            "praeto_mat_silver_crystal",
            "praeto_mat_frostbitten_fang",
            "praeto_mat_fuling_flesh",
            "praeto_mat_fuling_hex_totem",
            "praeto_mat_gold_scrap",
            "praeto_mat_dvergr_flesh",
            "praeto_mat_yggdrasil_branch",
            "praeto_mat_seeker_venom_gland"
        };

        [Test]
        public void All_contains_the_readme_ids_in_stable_order()
        {
            Assert.That(MaterialCatalog.All.Select(item => item.PrefabId), Is.EqualTo(ExpectedIds));
        }

        [Test]
        public void Every_definition_has_complete_unique_metadata()
        {
            Assert.Multiple(() =>
            {
                Assert.That(MaterialCatalog.All, Has.Count.EqualTo(22));
                Assert.That(MaterialCatalog.All.Select(item => item.PrefabId), Is.Unique);
                Assert.That(MaterialCatalog.All.Select(item => item.NameToken), Is.Unique);
                Assert.That(MaterialCatalog.All.Select(item => item.DescriptionToken), Is.Unique);
                Assert.That(MaterialCatalog.All.All(item => item.CloneSourcePrefab == "Amber"), Is.True);
                Assert.That(MaterialCatalog.All.All(item => !string.IsNullOrWhiteSpace(item.EnglishName)), Is.True);
                Assert.That(MaterialCatalog.All.All(item => !string.IsNullOrWhiteSpace(item.EnglishDescription)), Is.True);
                Assert.That(MaterialCatalog.All.All(item => item.NameToken.StartsWith("$suijin_", StringComparison.Ordinal)), Is.True);
                Assert.That(MaterialCatalog.All.All(item => item.DescriptionToken.EndsWith("_description", StringComparison.Ordinal)), Is.True);
                Assert.That(MaterialCatalog.All.All(item => item.IconFileName == item.PrefabId + ".png"), Is.True);
            });
        }

        [Test]
        public void Validate_reports_duplicate_ids_and_tokens()
        {
            MaterialDefinition first = Definition("praeto_mat_duplicate", "$suijin_first", "$suijin_first_description");
            MaterialDefinition duplicate = Definition("praeto_mat_duplicate", "$suijin_first", "$suijin_first_description");

            var errors = MaterialCatalogValidator.Validate(new[] { first, duplicate });

            Assert.That(errors, Has.Some.Contains("Duplicate prefab ID 'praeto_mat_duplicate'"));
            Assert.That(errors, Has.Some.Contains("Duplicate name token '$suijin_first'"));
            Assert.That(errors, Has.Some.Contains("Duplicate description token '$suijin_first_description'"));
        }

        [Test]
        public void Validate_reports_each_empty_required_field()
        {
            var invalid = new MaterialDefinition("", "", "", "", "", "", "");

            var errors = MaterialCatalogValidator.Validate(new[] { invalid });

            Assert.That(errors, Has.Count.EqualTo(7));
        }

        private static MaterialDefinition Definition(string prefabId, string nameToken, string descriptionToken)
        {
            return new MaterialDefinition(
                prefabId,
                nameToken,
                descriptionToken,
                "Test Material",
                "A test material.",
                prefabId + ".png",
                "Amber");
        }
    }
}
