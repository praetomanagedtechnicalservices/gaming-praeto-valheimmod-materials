using System; using System.Collections.Generic;
namespace SuijinMaterials.World
{
    public static class WorldMaterialCatalog
    {
        public static IReadOnlyList<WorldMaterialDefinition> All { get; } = Array.AsReadOnly(new[] {
            new WorldMaterialDefinition("yarrow-meadows", WorldMaterialKind.Vegetation, "praeto_mat_yarrow", "Pickable_Dandelion", new[]{"Pickable_Dandelion"}, 1, .35, 1, 2, 1, 240, 0, true),
            new WorldMaterialDefinition("nightbloom-swamp", WorldMaterialKind.VegetationReplacement, "praeto_mat_nightbloom", "Pickable_Thistle", new[]{"Pickable_Thistle"}, 2, 1, 1, 1, 1, 240, 0, true),
            new WorldMaterialDefinition("yggdrasil-branch-roots", WorldMaterialKind.PersistentRootNode, "praeto_mat_yggdrasil_branch", "YggdrasilShoot", new[]{"YggdrasilRoot","YggdrasilRoot_small1","YggdrasilRoot_small2"}, 512, .35, 1, 1, 1, 0, 0, false)
        });
    }
}
