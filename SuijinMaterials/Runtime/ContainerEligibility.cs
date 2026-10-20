using System;
using System.Collections.Generic;
using System.Linq;
using SuijinMaterials.Acquisition;

namespace SuijinMaterials.Runtime
{
    public sealed class ContainerFacts
    {
        public ContainerFacts(string prefabId, bool authoritative, bool natural, bool dungeonOrInterior, bool playerBuilt, bool populated)
        { PrefabId = prefabId ?? string.Empty; Authoritative = authoritative; Natural = natural; DungeonOrInterior = dungeonOrInterior; PlayerBuilt = playerBuilt; Populated = populated; }
        public string PrefabId { get; }
        public bool Authoritative { get; }
        public bool Natural { get; }
        public bool DungeonOrInterior { get; }
        public bool PlayerBuilt { get; }
        public bool Populated { get; }
    }

    public static class ContainerEligibility
    {
        public static bool Evaluate(ContainerFacts facts)
        {
            if (facts == null || !facts.Authoritative || !facts.Natural || facts.DungeonOrInterior || facts.PlayerBuilt || facts.Populated) return false;
            AcquisitionDefinition rule = AcquisitionCatalog.All.First(item => item.RuleId == "huginn-overworld-chest");
            return rule.SourcePrefabIds.Any(source => PrefabIdentity.Equals(source, facts.PrefabId));
        }
    }
}
