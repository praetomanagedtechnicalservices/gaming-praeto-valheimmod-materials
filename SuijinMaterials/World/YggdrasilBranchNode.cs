using UnityEngine;

namespace SuijinMaterials.World
{
    public sealed class YggdrasilBranchNode : MonoBehaviour, IDestructible
    {
        private IBranchHarvestRequestSink requestSink;

        internal void Configure(IBranchHarvestRequestSink sink)
        {
            requestSink = sink;
        }

        public DestructibleType GetDestructibleType() => DestructibleType.Tree;

        public void Damage(HitData hit)
        {
            if (hit == null || requestSink == null) return;
            Vector3 point = hit.m_point;
            BranchHitAdapter.Submit(
                requestSink,
                (int)hit.m_skill,
                hit.GetTotalPhysicalDamage(),
                hit.m_toolTier,
                point.x,
                point.y,
                point.z);
        }
    }
}
