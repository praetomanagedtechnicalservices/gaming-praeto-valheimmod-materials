using System;
using System.Collections.Generic;
namespace SuijinMaterials.World
{
    public sealed class WorldMaterialDefinition
    {
        public WorldMaterialDefinition(string ruleId, WorldMaterialKind kind, string materialPrefabId, string clonePrefabId, IEnumerable<string> sources, int biome, double chance, int groupMin, int groupMax, int yield, double respawnMinutes, int minimumToolTier, bool renewable)
        { RuleId=ruleId; Kind=kind; MaterialPrefabId=materialPrefabId; ClonePrefabId=clonePrefabId; SourcePrefabIds=Array.AsReadOnly(new List<string>(sources ?? Array.Empty<string>()).ToArray()); Biome=biome; DefaultChance=chance; DefaultGroupMin=groupMin; DefaultGroupMax=groupMax; DefaultYield=yield; DefaultRespawnMinutes=respawnMinutes; DefaultMinimumToolTier=minimumToolTier; Renewable=renewable; }
        public string RuleId { get; } public WorldMaterialKind Kind { get; } public string MaterialPrefabId { get; } public string ClonePrefabId { get; } public IReadOnlyList<string> SourcePrefabIds { get; } public int Biome { get; } public double DefaultChance { get; } public int DefaultGroupMin { get; } public int DefaultGroupMax { get; } public int DefaultYield { get; } public double DefaultRespawnMinutes { get; } public int DefaultMinimumToolTier { get; } public bool Renewable { get; }
    }
}
