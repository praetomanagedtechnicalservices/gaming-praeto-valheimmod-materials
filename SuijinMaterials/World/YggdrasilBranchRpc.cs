using System;

namespace SuijinMaterials.World
{
    public sealed class YggdrasilBranchRpc : IBranchHarvestRequestSink
    {
        public const string RpcName = "praeto.suijin.v05.branch.harvest";

        private readonly IBranchHarvestRpcPeer peer;
        private readonly Func<IYggdrasilRootState> state;
        private readonly Func<WorldRuleSettings> settings;
        private readonly Action<BranchHarvestRequest, int> emitLoot;
        private readonly Action<string> logError;
        private bool registered;

        public YggdrasilBranchRpc(
            IBranchHarvestRpcPeer peer,
            Func<IYggdrasilRootState> state,
            Func<WorldRuleSettings> settings,
            Action<BranchHarvestRequest, int> emitLoot,
            Action<string> logError)
        {
            this.peer = peer ?? throw new ArgumentNullException(nameof(peer));
            this.state = state ?? throw new ArgumentNullException(nameof(state));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.emitLoot = emitLoot ?? throw new ArgumentNullException(nameof(emitLoot));
            this.logError = logError ?? (_ => { });
        }

        public void Register()
        {
            if (registered) return;
            peer.Register(RpcName, Handle);
            registered = true;
        }

        public void Unregister()
        {
            if (!registered) return;
            peer.Unregister(RpcName);
            registered = false;
        }

        public void RequestHarvest(BranchHarvestRequest request)
        {
            if (request == null) return;
            peer.SendToOwner(RpcName, request);
        }

        private void Handle(long sender, BranchHarvestRequest request)
        {
            try
            {
                WorldRuleSettings current = settings();
                if (current == null || !current.Enabled) return;
                BranchHarvestRequestHandler.Handle(
                    peer.IsOwner,
                    state(),
                    request,
                    current.MinimumToolTier,
                    () => emitLoot(request, current.Yield));
            }
            catch (Exception exception)
            {
                logError("Yggdrasil Branch RPC failed for sender " + sender + ": " + exception.Message);
            }
        }
    }

    internal sealed class ZNetViewBranchHarvestRpcPeer : IBranchHarvestRpcPeer
    {
        private readonly ZNetView view;
        private readonly Action<string> logError;

        public ZNetViewBranchHarvestRpcPeer(ZNetView view, Action<string> logError)
        {
            this.view = view ?? throw new ArgumentNullException(nameof(view));
            this.logError = logError ?? (_ => { });
        }

        public bool IsOwner => view.IsOwner();

        public void Register(string name, Action<long, BranchHarvestRequest> handler)
        {
            view.Register<ZPackage>(name, (sender, package) =>
            {
                if (BranchHarvestRequestCodec.TryDecode(package, out BranchHarvestRequest request))
                {
                    handler(sender, request);
                }
                else
                {
                    logError("Rejected malformed Yggdrasil Branch RPC payload from sender " + sender + ".");
                }
            });
        }

        public void Unregister(string name) => view.Unregister(name);

        public void SendToOwner(string name, BranchHarvestRequest request)
        {
            view.InvokeRPC(name, BranchHarvestRequestCodec.Encode(request));
        }
    }
}
