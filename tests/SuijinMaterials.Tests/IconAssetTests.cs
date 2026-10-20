using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SuijinMaterials.Assets;
using SuijinMaterials.Content;

namespace SuijinMaterials.Tests
{
    [TestFixture]
    public sealed class IconAssetTests
    {
        private string sourcePluginDirectory;

        [OneTimeSetUp]
        public void FindSourcePluginDirectory()
        {
            DirectoryInfo current = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            while (current != null && !File.Exists(Path.Combine(current.FullName, "SuijinMaterials.sln")))
            {
                current = current.Parent;
            }

            Assert.That(current, Is.Not.Null, "Could not locate repository root from the test output directory.");
            sourcePluginDirectory = Path.Combine(current.FullName, "SuijinMaterials");
        }

        [Test]
        public void Every_catalog_icon_is_a_valid_64_pixel_png()
        {
            var failures = new List<string>();

            foreach (MaterialDefinition definition in MaterialCatalog.All)
            {
                string path = IconAsset.Resolve(sourcePluginDirectory, definition);
                if (!File.Exists(path))
                {
                    failures.Add($"Missing {definition.PrefabId}: {path}");
                    continue;
                }

                string error = IconAsset.ValidatePng(path);
                if (error != null)
                {
                    failures.Add(error);
                }
            }

            Assert.That(failures, Is.Empty);
        }

        [Test]
        public void Resolve_preserves_the_runtime_asset_layout()
        {
            MaterialDefinition definition = MaterialCatalog.All[0];

            string path = IconAsset.Resolve("C:\\mods\\SuijinMaterials", definition);

            Assert.That(path, Is.EqualTo(Path.GetFullPath("C:\\mods\\SuijinMaterials\\Assets\\Icons\\praeto_mat_huginn_feather.png")));
        }

        [Test]
        public void Resolve_rejects_icon_filename_traversal()
        {
            var definition = new MaterialDefinition(
                "praeto_mat_escape",
                "$suijin_escape",
                "$suijin_escape_description",
                "Escape",
                "Invalid test fixture.",
                "..\\escape.png",
                "Amber");

            Assert.That(
                () => IconAsset.Resolve("C:\\mods\\SuijinMaterials", definition),
                Throws.ArgumentException.With.Message.Contains("praeto_mat_escape"));
        }

        [Test]
        public void ValidatePng_reports_the_bad_asset_path()
        {
            string path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "praeto_mat_broken.png");
            File.WriteAllBytes(path, new byte[] { 1, 2, 3, 4 });

            try
            {
                string error = IconAsset.ValidatePng(path);

                Assert.That(error, Does.Contain("praeto_mat_broken.png"));
                Assert.That(error, Does.Contain("PNG"));
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Test]
        public void ValidatePng_rejects_non_64_pixel_dimensions()
        {
            string validPath = IconAsset.Resolve(sourcePluginDirectory, MaterialCatalog.All[0]);
            string path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "praeto_mat_wrong_size.png");
            byte[] bytes = File.ReadAllBytes(validPath);
            bytes[19] = 32;
            File.WriteAllBytes(path, bytes);

            try
            {
                Assert.That(IconAsset.ValidatePng(path), Does.Contain("64x64"));
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
