using System;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace SuijinMaterials.World
{
    public sealed class JotunnVegetationFacade : IVegetationFacade
    {
        private const string ThistleVegetationName = "Pickable_Thistle";

        public int GetThistleBiomeMask()
        {
            return (int)GetThistleVegetation().m_biome;
        }

        public void SetThistleBiomeMask(int biomeMask)
        {
            GetThistleVegetation().m_biome = (Heightmap.Biome)biomeMask;
        }

        public void AddYarrow(WorldRuleSettings settings)
        {
            GameObject prefab = CreatePickable(
                VegetationInstaller.YarrowVegetationName,
                "Pickable_Dandelion",
                "praeto_mat_yarrow",
                settings);

            VegetationConfig config = new VegetationConfig
            {
                Biome = Heightmap.Biome.Meadows,
                BiomeArea = Heightmap.BiomeArea.Everything,
                BlockCheck = true,
                ForcePlacement = false,
                Min = 0f,
                Max = (float)settings.Chance,
                MinAltitude = 0f,
                MaxAltitude = 1000f,
                MinOceanDepth = 0f,
                MaxOceanDepth = 0f,
                MinTerrainDelta = 0f,
                MaxTerrainDelta = 2f,
                MinTilt = 0f,
                MaxTilt = 35f,
                InForest = false,
                ForestThresholdMin = 0f,
                ForestThresholdMax = 1f,
                TerrainDeltaRadius = 0f,
                ScaleMin = 0.9f,
                ScaleMax = 1.1f,
                GroupSizeMin = settings.GroupMin,
                GroupSizeMax = settings.GroupMax,
                GroupRadius = 2f,
                GroundOffset = 0f
            };

            ZoneManager.Instance.AddCustomVegetation(new CustomVegetation(prefab, true, config));
        }

        public void AddNightbloom(WorldRuleSettings settings)
        {
            ZoneSystem.ZoneVegetation source = GetThistleVegetation();
            GameObject prefab = CreatePickable(
                VegetationInstaller.NightbloomVegetationName,
                "Pickable_Thistle",
                "praeto_mat_nightbloom",
                settings);

            VegetationConfig config = new VegetationConfig(source)
            {
                Biome = Heightmap.Biome.Swamp,
                Min = 0f,
                Max = (float)settings.Chance,
                GroupSizeMin = settings.GroupMin,
                GroupSizeMax = settings.GroupMax
            };

            ZoneManager.Instance.AddCustomVegetation(new CustomVegetation(prefab, true, config));
        }

        public void Remove(string name)
        {
            ZoneManager.Instance.RemoveCustomVegetation(name);
        }

        private static ZoneSystem.ZoneVegetation GetThistleVegetation()
        {
            return ZoneManager.Instance.GetZoneVegetation(ThistleVegetationName)
                ?? throw new InvalidOperationException("Vanilla Thistle vegetation was not available.");
        }

        private static GameObject CreatePickable(string cloneName, string sourceName, string itemName, WorldRuleSettings settings)
        {
            GameObject prefab = PrefabManager.Instance.CreateClonedPrefab(cloneName, sourceName);
            Pickable pickable = prefab.GetComponent<Pickable>()
                ?? throw new InvalidOperationException(sourceName + " does not contain Pickable.");
            pickable.m_itemPrefab = PrefabManager.Instance.GetPrefab(itemName)
                ?? throw new InvalidOperationException(itemName + " was not registered.");
            pickable.m_amount = settings.Yield;
            pickable.m_respawnTimeMinutes = (float)settings.RespawnMinutes;
            return prefab;
        }
    }
}
