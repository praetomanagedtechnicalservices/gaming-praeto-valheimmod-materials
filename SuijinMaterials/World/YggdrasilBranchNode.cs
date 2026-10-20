using Jotunn.Managers;
using UnityEngine;

namespace SuijinMaterials.World
{
    public sealed class YggdrasilBranchNode : MonoBehaviour, IDestructible
    {
        private ZNetView root;
        private int yield;
        private int minimumToolTier;

        internal void Configure(ZNetView rootView, int itemYield, int toolTier)
        {
            root = rootView;
            yield = itemYield;
            minimumToolTier = toolTier;
        }

        public DestructibleType GetDestructibleType() => DestructibleType.Tree;

        public void Damage(HitData hit)
        {
            if (hit == null || root == null || !root.IsValid()) return;
            ZDO zdo = root.GetZDO();
            if (zdo == null) return;
            ZdoYggdrasilRootState state = new ZdoYggdrasilRootState(zdo);
            bool isChop = hit.m_skill == Skills.SkillType.Axes || hit.m_skill == Skills.SkillType.WoodCutting;
            BranchHarvestContext context = new BranchHarvestContext(root.IsOwner(), isChop, hit.GetTotalPhysicalDamage(), hit.m_toolTier, minimumToolTier);
            BranchHarvestResult result = YggdrasilBranchPolicy.TryHarvest(state, context, () => EmitLoot());
            if (result == BranchHarvestResult.Harvested) UnityEngine.Object.Destroy(gameObject);
        }

        private void EmitLoot()
        {
            GameObject prefab = PrefabManager.Instance.GetPrefab("praeto_mat_yggdrasil_branch");
            ItemDrop drop = prefab == null ? null : prefab.GetComponent<ItemDrop>();
            if (drop == null) return;
            ItemDrop.DropItem(drop.m_itemData.Clone(), yield, transform.position, Quaternion.identity);
        }
    }
}
