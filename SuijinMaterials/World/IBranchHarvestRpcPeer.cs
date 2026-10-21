using System;

namespace SuijinMaterials.World
{
    public interface IBranchHarvestRpcPeer
    {
        bool IsOwner { get; }
        void Register(string name, Action<long, BranchHarvestRequest> handler);
        void Unregister(string name);
        void SendToOwner(string name, BranchHarvestRequest request);
    }
}
