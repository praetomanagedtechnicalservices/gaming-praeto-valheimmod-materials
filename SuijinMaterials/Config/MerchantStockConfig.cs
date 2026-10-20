using System; using System.Collections.Generic; using System.Linq; using BepInEx.Configuration; using Jotunn.Extensions; using SuijinMaterials.Merchants;
namespace SuijinMaterials.Config
{
 public sealed class MerchantStockConfig
 {
  private readonly Dictionary<string,E> entries; private MerchantStockConfig(Dictionary<string,E> e){entries=e;}
  public static MerchantStockConfig Bind(ConfigFile c){var d=new Dictionary<string,E>();foreach(var x in MerchantStockCatalog.All){string s="Merchant - "+x.ListingId;d[x.ListingId]=new E{Enabled=c.BindConfig(s,"Enabled",true,"Server-authoritative merchant listing.",synced:true),Price=c.BindConfig(s,"Price",x.Price,"Coin price.",synced:true,acceptableValues:new AcceptableValueRange<int>(1,100000)),Stack=c.BindConfig(s,"Purchase Stack",x.Stack,"Items per purchase.",synced:true,acceptableValues:new AcceptableValueRange<int>(1,100))};}return new MerchantStockConfig(d);}
  public MerchantStockSettings Snapshot()=>new MerchantStockSettings(entries.ToDictionary(p=>p.Key,p=>new MerchantListingSettings(p.Value.Enabled.Value,Math.Max(1,p.Value.Price.Value),Math.Max(1,p.Value.Stack.Value))));
  public static MerchantStockSettings CreateDefaultSnapshot()=>new MerchantStockSettings(MerchantStockCatalog.All.ToDictionary(x=>x.ListingId,x=>new MerchantListingSettings(true,x.Price,x.Stack)));
  private sealed class E{public ConfigEntry<bool> Enabled;public ConfigEntry<int> Price,Stack;}
 }
}
