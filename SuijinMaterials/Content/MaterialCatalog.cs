using System;
using System.Collections.Generic;

namespace SuijinMaterials.Content
{
    public static class MaterialCatalog
    {
        public static IReadOnlyList<MaterialDefinition> All { get; } = Array.AsReadOnly(new[]
        {
            Define("huginn_feather", "Huginn's Feather", "A dark feather carrying the memory of distant travels."),
            Define("gull_feather", "Gull Feather", "A salt-touched feather with a pale coastal sheen."),
            Define("crow_feather", "Crow Feather", "A glossy black feather from a watchful crow."),
            Define("ash_crow_feather", "Ash Crow Feather", "A soot-dark feather hardened by the heat of the Ashlands."),
            Define("munin_feather", "Munin Feather", "A rare feather imbued with memory and quiet insight."),
            Define("hen_feather", "Hen Feather", "A soft feather suited to careful handiwork."),
            Define("aged_tissue", "Aged Tissue", "Dense tissue preserved by time and lingering magic."),
            Define("voodoo_doll", "Voodoo Doll", "A small bound figure heavy with unsettling intent."),
            Define("canvas", "Canvas", "A sturdy woven cloth prepared for practical craft."),
            Define("yarrow", "Yarrow", "A fragrant meadow herb prized by healers and foragers."),
            Define("elderroot", "Elderroot", "An ancient root threaded with the forest's old power."),
            Define("nightbloom", "Nightbloom", "A shadowed flower that keeps its color in the deepest mire."),
            Define("rotten_elderroot", "Rotten Elderroot", "A decaying root still knotted with stubborn strength."),
            Define("runecarved_bone", "Runecarved Bone", "An old bone scored with weathered runes."),
            Define("silver_crystal", "Silver Crystal", "A bright mineral shard with a cold metallic gleam."),
            Define("frostbitten_fang", "Frostbitten Fang", "A predator's fang made brittle and keen by mountain frost."),
            Define("fuling_flesh", "Fuling Flesh", "Tough green flesh with an acrid, smoky scent."),
            Define("fuling_hex_totem", "Fuling Hex Totem", "A crude idol marked with hostile ritual signs."),
            Define("gold_scrap", "Gold Scrap", "A bent fragment of workable gold."),
            Define("dvergr_flesh", "Dvergr Flesh", "Dense flesh carrying a faint trace of refined eitr."),
            Define("yggdrasil_branch", "Yggdrasil Branch", "A living branch cut from the roots of the world tree."),
            Define("seeker_venom_gland", "Seeker Venom Gland", "A resilient gland filled with caustic seeker venom.")
        });

        private static MaterialDefinition Define(string suffix, string englishName, string englishDescription)
        {
            string prefabId = "praeto_mat_" + suffix;
            string nameToken = "$suijin_" + suffix;

            return new MaterialDefinition(
                prefabId,
                nameToken,
                nameToken + "_description",
                englishName,
                englishDescription,
                prefabId + ".png",
                "Amber");
        }
    }
}
