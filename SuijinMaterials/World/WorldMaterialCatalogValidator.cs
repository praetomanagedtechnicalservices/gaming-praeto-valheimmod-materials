using System; using System.Collections.Generic; using System.Linq; using SuijinMaterials.Content;
namespace SuijinMaterials.World
{
    public static class WorldMaterialCatalogValidator
    {
        public static IReadOnlyList<string> Validate(IEnumerable<WorldMaterialDefinition> definitions)
        {
            var errors=new List<string>(); var all=(definitions??Array.Empty<WorldMaterialDefinition>()).ToArray();
            foreach(var g in all.GroupBy(x=>x.RuleId).Where(x=>x.Count()>1)) errors.Add("Duplicate world rule ID '"+g.Key+"'.");
            var known=new HashSet<string>(MaterialCatalog.All.Select(x=>x.PrefabId));
            foreach(var x in all){ if(!known.Contains(x.MaterialPrefabId)) errors.Add(x.RuleId+" references unknown material."); if(x.DefaultChance<0||x.DefaultChance>1) errors.Add(x.RuleId+" has invalid probability."); if(x.DefaultGroupMin<1||x.DefaultGroupMax<x.DefaultGroupMin||x.DefaultYield<1) errors.Add(x.RuleId+" has invalid range."); if(x.SourcePrefabIds.Count==0) errors.Add(x.RuleId+" requires explicit sources."); if(x.Kind==WorldMaterialKind.PersistentRootNode&&x.Renewable) errors.Add(x.RuleId+" cannot be renewable."); }
            return errors.AsReadOnly();
        }
    }
}
