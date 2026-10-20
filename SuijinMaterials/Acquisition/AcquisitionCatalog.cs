using System;
using System.Collections.Generic;

namespace SuijinMaterials.Acquisition
{
    public static class AcquisitionCatalog
    {
        private static readonly string[] OverworldChests =
        {
            "TreasureChest_meadows", "TreasureChest_blackforest", "TreasureChest_swamp",
            "TreasureChest_mountains", "TreasureChest_plains_stone", "TreasureChest_plains",
            "TreasureChest_dvergrtower", "TreasureChest_dvergrtown"
        };

        private static readonly string[] MistlandsRuins =
        {
            "Mistlands_Excavation1", "Mistlands_Excavation2", "Mistlands_Excavation3",
            "Mistlands_GuardTower1_new", "Mistlands_GuardTower1_ruined_new",
            "Mistlands_GuardTower1_ruined_new2", "Mistlands_GuardTower2_new",
            "Mistlands_GuardTower3_new", "Mistlands_GuardTower3_ruined_new",
            "Mistlands_Harbour1", "Mistlands_Lighthouse1_new"
        };

        public static IReadOnlyList<AcquisitionDefinition> All { get; } = Array.AsReadOnly(new[]
        {
            Rule("huginn-overworld-chest", AcquisitionRuleKind.AddContainerLoot, "praeto_mat_huginn_feather", OverworldChests, 0.12),
            Rule("hen-feather-conversion", AcquisitionRuleKind.ReplaceCreatureDrop, "praeto_mat_hen_feather", new[] { "Hen" }, 1.0, replaced: "Feathers"),
            Rule("elderroot-greydwarf-nest", AcquisitionRuleKind.AddedDestructibleDrop, "praeto_mat_elderroot", new[] { "Spawner_GreydwarfNest" }, 0.25),
            Rule("elderroot-elder", AcquisitionRuleKind.AddedBossDrop, "praeto_mat_elderroot", new[] { "gd_king" }, 1.0, 1, 4),
            Rule("rotten-elderroot-abomination", AcquisitionRuleKind.AddedCreatureDrop, "praeto_mat_rotten_elderroot", new[] { "Abomination" }, 0.25),
            Rule("runecarved-bone-skeleton-pile", AcquisitionRuleKind.AddedDestructibleDrop, "praeto_mat_runecarved_bone", new[] { "BonePileSpawner", "BonePileSpawner_swamp" }, 1.0),
            Rule("silver-crystal-stone-golem", AcquisitionRuleKind.ReplaceCreatureDrop, "praeto_mat_silver_crystal", new[] { "StoneGolem" }, 1.0, replaced: "Crystal"),
            Rule("frostbitten-fang-fenring", AcquisitionRuleKind.AddedCreatureDrop, "praeto_mat_frostbitten_fang", new[] { "Fenring" }, 0.20),
            Rule("frostbitten-fang-wolf-one-star", AcquisitionRuleKind.AddedCreatureDrop, "praeto_mat_frostbitten_fang", new[] { "Wolf" }, 0.15, sourceLevel: 2),
            Rule("frostbitten-fang-wolf-two-star", AcquisitionRuleKind.AddedCreatureDrop, "praeto_mat_frostbitten_fang", new[] { "Wolf" }, 0.30, sourceLevel: 3),
            Rule("fuling-flesh", AcquisitionRuleKind.AddedCreatureDrop, "praeto_mat_fuling_flesh", new[] { "Goblin", "GoblinBrute", "GoblinShaman" }, 0.20),
            Rule("fuling-hex-totem", AcquisitionRuleKind.AddedCreatureDrop, "praeto_mat_fuling_hex_totem", new[] { "GoblinShaman" }, 0.10),
            Rule("gold-scrap-mistlands-ruin", AcquisitionRuleKind.ReplaceContainerLoot, "praeto_mat_gold_scrap", new[] { "dvergrprops_lantern" }, 1.0, locations: MistlandsRuins, replaced: "CopperScrap"),
            Rule("dvergr-flesh", AcquisitionRuleKind.AddedCreatureDrop, "praeto_mat_dvergr_flesh", new[] { "Dverger", "DvergerMage" }, 0.15),
            Rule("seeker-venom-gland-seeker", AcquisitionRuleKind.AddedCreatureDrop, "praeto_mat_seeker_venom_gland", new[] { "Seeker" }, 0.15),
            Rule("seeker-venom-gland-soldier", AcquisitionRuleKind.AddedCreatureDrop, "praeto_mat_seeker_venom_gland", new[] { "SeekerBrute" }, 0.25),
            Disabled("gull-feather-disabled", "praeto_mat_gull_feather"),
            Disabled("crow-feather-disabled", "praeto_mat_crow_feather"),
            Disabled("ash-crow-feather-disabled", "praeto_mat_ash_crow_feather"),
            Disabled("munin-feather-disabled", "praeto_mat_munin_feather")
        });

        private static AcquisitionDefinition Rule(string id, AcquisitionRuleKind kind, string material, string[] sources, double chance, int min = 1, int max = 1, int sourceLevel = 0, string[] locations = null, string replaced = null)
        {
            return new AcquisitionDefinition(id, kind, material, sources, locations ?? Array.Empty<string>(), chance, min, max, sourceLevel, replaced);
        }

        private static AcquisitionDefinition Disabled(string id, string material)
        {
            return new AcquisitionDefinition(id, AcquisitionRuleKind.Disabled, material, Array.Empty<string>(), Array.Empty<string>(), 0, 1, 1, disabledReason: "Deferred until a later creature integration provides a verified source.");
        }
    }
}
