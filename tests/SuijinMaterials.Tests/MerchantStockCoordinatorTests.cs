using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SuijinMaterials.Merchants;

namespace SuijinMaterials.Tests
{
    [TestFixture]
    public sealed class MerchantStockCoordinatorTests
    {
        [Test]
        public void Refresh_replaces_owned_entries_without_duplication()
        {
            FakeFacade facade = new FakeFacade();
            MerchantStockInstaller installer = new MerchantStockInstaller(facade, _ => { });
            MerchantStockSettings settings = new MerchantStockSettings(MerchantStockCatalog.All.ToDictionary(x => x.ListingId, x => new MerchantListingSettings(true, x.Price, x.Stack)));
            MerchantStockCoordinator coordinator = new MerchantStockCoordinator(() => settings, installer);
            coordinator.Install();
            coordinator.Refresh();
            Assert.That(facade.Handles.Count, Is.EqualTo(3));
            coordinator.Uninstall();
            Assert.That(facade.Handles, Is.Empty);
        }

        private sealed class FakeFacade : IMerchantStockFacade
        {
            public readonly HashSet<object> Handles = new HashSet<object>();
            public object Add(MerchantStockDefinition definition, MerchantListingSettings settings) { object handle = new object(); Handles.Add(handle); return handle; }
            public void Remove(object handle) { Handles.Remove(handle); }
        }
    }
}
