namespace SuijinMaterials.Merchants { public interface IMerchantStockFacade { object Add(MerchantStockDefinition definition,MerchantListingSettings settings); void Remove(object handle); } }
