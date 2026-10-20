using System.Linq;
using NUnit.Framework;
using SuijinMaterials.Merchants;

namespace SuijinMaterials.Tests
{
    [TestFixture]
    public sealed class MerchantStockCatalogTests
    {
        [Test]
        public void Catalog_has_exact_approved_vanilla_stock()
        {
            Assert.That(MerchantStockCatalog.All.Select(x => x.TraderPrefabId), Is.EqualTo(new[] { "Haldor", "Hildir", "BogWitch" }));
            Assert.That(MerchantStockCatalog.All.Select(x => x.Price), Is.EqualTo(new[] { 240, 60, 500 }));
            Assert.That(MerchantStockCatalog.All.Select(x => x.RequiredGlobalKey), Is.EqualTo(new[] { "defeated_dragon", "", "defeated_goblinking" }));
            Assert.That(MerchantStockCatalog.All.Select(x => x.MaterialPrefabId), Is.EqualTo(new[] { "praeto_mat_aged_tissue", "praeto_mat_canvas", "praeto_mat_voodoo_doll" }));
            Assert.That(MerchantStockCatalogValidator.Validate(MerchantStockCatalog.All), Is.Empty);
        }
    }
}
